using System.Data;
using Dapper;
using FocusMind.DBContext.Common;
using FocusMind.DBEntity;
using Microsoft.Data.SqlClient;

namespace FocusMind.DBContext.Repositories;

public sealed class PedidoRepository(IDbConnectionFactory connectionFactory) : IPedidoRepository
{
    public async Task<IEnumerable<Pedido>> ListarXUsuarioAsync(int idUsuario)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDUSUARIO", idUsuario);

        return await connection.QueryAsync<Pedido>(
            "usp_Listar_Pedido_X_Usuario",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Pedido?> ObtenerAsync(int idPedido)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDPEDIDO", idPedido);

        return await connection.QuerySingleOrDefaultAsync<Pedido>(
            "usp_ObtenerPedido",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<PedidoDetalle>> ListarDetalleAsync(int idPedido)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDPEDIDO", idPedido);

        return await connection.QueryAsync<PedidoDetalle>(
            "usp_Listar_PedidoDetalle_X_Pedido",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    // ════════════════════════════════════════════════════════════════════════════════════
    // TRANSACCIÓN ACID (HU-18): usp_InsertarPedido + N x usp_InsertarPedidoDetalle.
    //
    // GOTCHA DE TRANSACCIONES ANIDADAS (documentado para que nadie lo "arregle" sin entender
    // por qué está así): usp_InsertarPedidoDetalle YA abre su propia transacción interna (ver
    // 02_Schema_ProcedimientosAlmacenados.sql, nota de diseño #7) para descontar STOCK de forma
    // atómica. Al invocarla dentro de ESTA transacción externa, SQL Server anida los BEGIN TRAN
    // (@@TRANCOUNT sube/baja con cada BEGIN/COMMIT interno). Pero si el SP detecta stock
    // insuficiente, hace un ROLLBACK TRANSACTION SIN nombre de savepoint — y ese tipo de
    // ROLLBACK deshace TODOS los niveles anidados de una sola vez, no solo el suyo. Esto es
    // EXACTAMENTE la atomicidad total que pide HU-18 (revierte también el usp_InsertarPedido ya
    // ejecutado y cualquier línea de detalle previa de este mismo pedido) — pero dejaría el
    // objeto SqlTransaction de .NET "huérfano": el servidor ya revirtió, pero el objeto managed
    // todavía no lo sabe. Por eso RollbackSilencioso() envuelve transaction.Rollback() en su
    // propio try/catch: si ya no queda nada que revertir, SqlClient lanza
    // InvalidOperationException("This SqlTransaction has completed..."), que se descarta a
    // propósito para no enmascarar la excepción real (stock insuficiente / conflicto de
    // NUMEROPEDIDO) que sí debe propagarse al llamador.
    // ════════════════════════════════════════════════════════════════════════════════════
    public async Task<int> ConfirmarAsync(Pedido pedido, IEnumerable<PedidoDetalle> detalle, int? usuarioCrea)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var parametrosPedido = new DynamicParameters();
            parametrosPedido.Add("@IDUSUARIO", pedido.IdUsuario);
            parametrosPedido.Add("@NUMEROPEDIDO", pedido.NumeroPedido);
            parametrosPedido.Add("@FECHAPEDIDO", pedido.FechaPedido);
            parametrosPedido.Add("@TOTAL", pedido.Total);
            parametrosPedido.Add("@NOMBRECLIENTE", pedido.NombreCliente);
            parametrosPedido.Add("@DIRECCIONENVIO", pedido.DireccionEnvio);
            parametrosPedido.Add("@CIUDADENVIO", pedido.CiudadEnvio);
            parametrosPedido.Add("@TELEFONOCONTACTO", pedido.TelefonoContacto);
            parametrosPedido.Add("@METODOPAGO", pedido.MetodoPago);
            parametrosPedido.Add("@USUARIOCREA", usuarioCrea);
            parametrosPedido.Add("@IDPEDIDO", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "usp_InsertarPedido",
                parametrosPedido,
                transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var idPedido = parametrosPedido.Get<int>("@IDPEDIDO");

            foreach (var item in detalle)
            {
                var parametrosDetalle = new DynamicParameters();
                parametrosDetalle.Add("@IDPEDIDO", idPedido);
                parametrosDetalle.Add("@IDPRODUCTO", item.IdProducto);
                parametrosDetalle.Add("@NOMBREPRODUCTO", item.NombreProducto);
                parametrosDetalle.Add("@PRECIOUNITARIO", item.PrecioUnitario);
                parametrosDetalle.Add("@CANTIDAD", item.Cantidad);
                parametrosDetalle.Add("@USUARIOCREA", usuarioCrea);
                parametrosDetalle.Add("@IDPEDIDODETALLE", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "usp_InsertarPedidoDetalle",
                    parametrosDetalle,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }

            transaction.Commit();
            return idPedido;
        }
        catch (SqlException ex) when (ex.Number == 51001)
        {
            RollbackSilencioso(transaction);
            throw new StockInsuficienteException("Stock insuficiente para completar la venta de uno o más productos del pedido.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            RollbackSilencioso(transaction);
            throw new NumeroPedidoDuplicadoException();
        }
        catch
        {
            RollbackSilencioso(transaction);
            throw;
        }
    }

    private static void RollbackSilencioso(IDbTransaction transaction)
    {
        try
        {
            transaction.Rollback();
        }
        catch (InvalidOperationException)
        {
            // La transacción ya fue revertida dentro de usp_InsertarPedidoDetalle
            // (ROLLBACK TRANSACTION sin savepoint deshace todos los niveles anidados).
        }
    }
}
