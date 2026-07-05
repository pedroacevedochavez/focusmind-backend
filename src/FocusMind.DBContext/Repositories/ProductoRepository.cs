using System.Data;
using Dapper;
using FocusMind.DBContext.Common;
using FocusMind.DBEntity;
using Microsoft.Data.SqlClient;

namespace FocusMind.DBContext.Repositories;

public sealed class ProductoRepository(IDbConnectionFactory connectionFactory) : IProductoRepository
{
    public async Task<IEnumerable<Producto>> ListarAsync()
    {
        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Producto>(
            "usp_ListarProducto",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Producto?> ObtenerAsync(int idProducto)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDPRODUCTO", idProducto);

        return await connection.QuerySingleOrDefaultAsync<Producto>(
            "usp_ObtenerProducto",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ProductoIngrediente>> ListarIngredientesAsync(int idProducto)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDPRODUCTO", idProducto);

        return await connection.QueryAsync<ProductoIngrediente>(
            "usp_Listar_ProductoIngrediente_X_Producto",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ProductoContraindicacion>> ListarContraindicacionesAsync(int idProducto)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDPRODUCTO", idProducto);

        return await connection.QueryAsync<ProductoContraindicacion>(
            "usp_Listar_ProductoContraindicacion_X_Producto",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Alergeno>> ListarAlergenosAsync(int idProducto)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDPRODUCTO", idProducto);

        return await connection.QueryAsync<Alergeno>(
            "usp_Listar_ProductoAlergeno_X_Producto",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    // Inserta el producto + sus hijos (ingredientes, contraindicaciones, alérgenos) en una sola
    // transacción ADO.NET: si cualquier INSERT falla (ej. un IDALERGENO inexistente), se revierte
    // todo — no queda un producto "a medias" sin sus hijos.
    public async Task<int> InsertarAsync(
        Producto producto,
        IEnumerable<string> ingredientes,
        IEnumerable<string> contraindicaciones,
        IEnumerable<int> alergenoIds,
        int? usuarioCrea)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var parametrosProducto = new DynamicParameters();
            parametrosProducto.Add("@NOMBRE", producto.Nombre);
            parametrosProducto.Add("@MARCA", producto.Marca);
            parametrosProducto.Add("@IDCATEGORIA", producto.IdCategoria);
            parametrosProducto.Add("@IDOBJETIVO", producto.IdObjetivo);
            parametrosProducto.Add("@PRECIO", producto.Precio);
            parametrosProducto.Add("@DESCRIPCION", producto.Descripcion);
            parametrosProducto.Add("@DOSISRECOMENDADA", producto.DosisRecomendada);
            parametrosProducto.Add("@URLIMAGEN", producto.UrlImagen);
            parametrosProducto.Add("@REGISTROSANITARIO", producto.RegistroSanitario);
            parametrosProducto.Add("@ENTIDADREGISTRO", producto.EntidadRegistro);
            parametrosProducto.Add("@STOCK", producto.Stock);
            parametrosProducto.Add("@USUARIOCREA", usuarioCrea);
            parametrosProducto.Add("@IDPRODUCTO", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "usp_InsertarProducto",
                parametrosProducto,
                transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var idProducto = parametrosProducto.Get<int>("@IDPRODUCTO");

            foreach (var ingrediente in ingredientes)
            {
                var parametrosIngrediente = new DynamicParameters();
                parametrosIngrediente.Add("@IDPRODUCTO", idProducto);
                parametrosIngrediente.Add("@INGREDIENTE", ingrediente);
                parametrosIngrediente.Add("@USUARIOCREA", usuarioCrea);
                parametrosIngrediente.Add("@IDPRODUCTOINGREDIENTE", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "usp_InsertarProductoIngrediente",
                    parametrosIngrediente,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }

            foreach (var contraindicacion in contraindicaciones)
            {
                var parametrosContraindicacion = new DynamicParameters();
                parametrosContraindicacion.Add("@IDPRODUCTO", idProducto);
                parametrosContraindicacion.Add("@DESCRIPCION", contraindicacion);
                parametrosContraindicacion.Add("@USUARIOCREA", usuarioCrea);
                parametrosContraindicacion.Add("@IDPRODUCTOCONTRAINDICACION", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "usp_InsertarProductoContraindicacion",
                    parametrosContraindicacion,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }

            foreach (var idAlergeno in alergenoIds)
            {
                var parametrosAlergeno = new DynamicParameters();
                parametrosAlergeno.Add("@IDPRODUCTO", idProducto);
                parametrosAlergeno.Add("@IDALERGENO", idAlergeno);
                parametrosAlergeno.Add("@USUARIOCREA", usuarioCrea);
                parametrosAlergeno.Add("@IDPRODUCTOALERGENO", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "usp_InsertarProductoAlergeno",
                    parametrosAlergeno,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }

            transaction.Commit();
            return idProducto;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    // usp_ActualizarProducto hace SET absoluto de todos los campos editables (incluido STOCK) —
    // no usar este método para aplicar deltas de stock concurrentes (ver usp_ActualizarStockProducto
    // en el SQL, todavía no expuesto vía API en esta historia).
    public async Task<bool> ActualizarAsync(Producto producto, int? usuarioModifica)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDPRODUCTO", producto.IdProducto);
        parametros.Add("@NOMBRE", producto.Nombre);
        parametros.Add("@MARCA", producto.Marca);
        parametros.Add("@IDCATEGORIA", producto.IdCategoria);
        parametros.Add("@IDOBJETIVO", producto.IdObjetivo);
        parametros.Add("@PRECIO", producto.Precio);
        parametros.Add("@DESCRIPCION", producto.Descripcion);
        parametros.Add("@DOSISRECOMENDADA", producto.DosisRecomendada);
        parametros.Add("@URLIMAGEN", producto.UrlImagen);
        parametros.Add("@REGISTROSANITARIO", producto.RegistroSanitario);
        parametros.Add("@ENTIDADREGISTRO", producto.EntidadRegistro);
        parametros.Add("@STOCK", producto.Stock);
        parametros.Add("@ACTIVO", producto.Activo);
        parametros.Add("@USUARIOMODIFICA", usuarioModifica);

        try
        {
            await connection.ExecuteAsync(
                "usp_ActualizarProducto",
                parametros,
                commandType: CommandType.StoredProcedure);

            return true;
        }
        catch (SqlException ex) when (ex.Number == 51002)
        {
            // THROW 51002 del SP: no existe ningún producto con ese IDPRODUCTO.
            return false;
        }
    }

    // Baja lógica: reutiliza usp_ActualizarProducto (no existe un usp_EliminarProducto dedicado
    // para no requerir un despliegue adicional sobre la instancia RDS ya operativa) leyendo primero
    // el producto vigente y volviendo a grabarlo con ACTIVO = 0.
    public async Task<bool> EliminarAsync(int idProducto, int? usuarioModifica)
    {
        var producto = await ObtenerAsync(idProducto);
        if (producto is null)
        {
            return false;
        }

        producto.Activo = false;
        return await ActualizarAsync(producto, usuarioModifica);
    }
}
