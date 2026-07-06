using System.Text.RegularExpressions;
using FocusMind.DBContext.Repositories;
using FocusMind.DBEntity;
using FocusMind.DTO.Requests;
using FocusMind.DTO.Responses;

namespace FocusMind.Business.Services;

public sealed class PedidoService(
    IPedidoRepository pedidoRepository,
    IProductoRepository productoRepository,
    IDisponibilidadService disponibilidadService) : IPedidoService
{
    private static readonly string[] MetodosPagoValidos = ["tarjeta", "yape", "contraentrega"];

    // ════════════════════════════════════════════════════════════════════════════════════
    // HU-18 — flujo completo, en 3 pasos:
    //   1) Validaciones de forma (método de pago / tarjeta) + reutiliza IDisponibilidadService
    //      (HU-17) como fail-fast ANTES de tocar TM_PEDIDO/TD_PEDIDO_DETALLE.
    //   2) Congela nombre + precio unitario vigente por cada ítem (snapshot histórico, ver
    //      nota de diseño de TD_PEDIDO_DETALLE en 01_Schema_Tablas.sql) y calcula el total.
    //   3) Delega a IPedidoRepository.ConfirmarAsync, que abre LA transacción ACID real.
    //      usp_InsertarPedidoDetalle vuelve a validar+descontar stock de forma atómica dentro
    //      de esa misma transacción: es la defensa real contra condiciones de carrera entre el
    //      paso 1 (chequeo) y este paso (escritura) — el paso 1 solo evita construir/enviar un
    //      pedido que ya sabemos que va a fallar.
    //
    // NOTA DE ALCANCE (costo aceptado): el producto de cada ítem se lee dos veces — una dentro
    // de IDisponibilidadService.ValidarAsync (paso 1) y otra aquí mismo para el snapshot (paso
    // 2) — en vez de que ValidarAsync devuelva también los Producto ya cargados. Se prefirió
    // así para reutilizar HU-17 tal cual (sin tener que ampliar su contrato) en vez de duplicar
    // su lógica de validación; el costo es aceptable para un carrito de pocos ítems.
    // ════════════════════════════════════════════════════════════════════════════════════
    public async Task<ResultadoPedido> ConfirmarPedidoAsync(PedidoCrearRequestDto dto, int idUsuario)
    {
        var errorMetodoPago = ValidarMetodoPago(dto.MetodoPago, dto.NumeroTarjeta);
        if (errorMetodoPago is not null)
        {
            return ResultadoPedido.ErrorValidacion(errorMetodoPago);
        }

        var disponibilidad = await disponibilidadService.ValidarAsync(dto.Items);
        if (!disponibilidad.TodoDisponible)
        {
            var motivos = disponibilidad.Items
                .Where(i => !i.Disponible)
                .Select(i => $"Producto {i.IdProducto}: {i.Motivo}");

            return ResultadoPedido.ErrorValidacion(
                "Uno o más productos ya no están disponibles. " + string.Join(" ", motivos));
        }

        var detalle = new List<PedidoDetalle>();
        var total = 0m;

        foreach (var item in dto.Items)
        {
            var producto = await productoRepository.ObtenerAsync(item.IdProducto);

            // Revalidación defensiva: ya pasó por IDisponibilidadService, pero podría haber
            // cambiado entre el paso 1 y este bucle (misma ventana de carrera que cierra, en
            // definitiva, la transacción de usp_InsertarPedidoDetalle).
            if (producto is null || !producto.Activo)
            {
                return ResultadoPedido.ErrorValidacion($"El producto {item.IdProducto} ya no está disponible.");
            }

            detalle.Add(new PedidoDetalle
            {
                IdProducto = producto.IdProducto,
                NombreProducto = producto.Nombre,
                PrecioUnitario = producto.Precio,
                Cantidad = item.Cantidad,
            });

            total += producto.Precio * item.Cantidad;
        }

        var pedido = new Pedido
        {
            IdUsuario = idUsuario,
            NumeroPedido = dto.NumeroPedido,
            FechaPedido = DateTime.UtcNow,
            Total = total,
            NombreCliente = dto.NombreCliente,
            DireccionEnvio = dto.DireccionEnvio,
            CiudadEnvio = dto.CiudadEnvio,
            TelefonoContacto = dto.TelefonoContacto,
            MetodoPago = dto.MetodoPago,
            Activo = true,
        };

        int idPedido;
        try
        {
            idPedido = await pedidoRepository.ConfirmarAsync(pedido, detalle, idUsuario);
        }
        catch (StockInsuficienteException ex)
        {
            return ResultadoPedido.ErrorValidacion(ex.Message);
        }
        catch (NumeroPedidoDuplicadoException ex)
        {
            return ResultadoPedido.ErrorConflicto(ex.Message);
        }

        return ResultadoPedido.Ok(MapearRespuesta(idPedido, pedido, detalle));
    }

    public async Task<IEnumerable<PedidoListItemResponseDto>> ListarXUsuarioAsync(int idUsuario)
    {
        var pedidos = await pedidoRepository.ListarXUsuarioAsync(idUsuario);

        return pedidos.Select(p => new PedidoListItemResponseDto
        {
            IdPedido = p.IdPedido,
            NumeroPedido = p.NumeroPedido,
            FechaPedido = p.FechaPedido,
            Total = p.Total,
            DireccionEnvio = p.DireccionEnvio,
            CiudadEnvio = p.CiudadEnvio,
            MetodoPago = p.MetodoPago,
        });
    }

    // Devuelve null tanto si el pedido no existe como si pertenece a otro usuario — el
    // controller responde 404 en ambos casos (nunca 403), para no confirmarle a un usuario
    // autenticado que un IDPEDIDO ajeno existe.
    public async Task<PedidoResponseDto?> ObtenerAsync(int idPedido, int idUsuarioSolicitante)
    {
        var pedido = await pedidoRepository.ObtenerAsync(idPedido);
        if (pedido is null || pedido.IdUsuario != idUsuarioSolicitante)
        {
            return null;
        }

        var detalle = await pedidoRepository.ListarDetalleAsync(idPedido);

        return MapearRespuesta(pedido, detalle);
    }

    private static string? ValidarMetodoPago(string metodoPago, string? numeroTarjeta)
    {
        if (!MetodosPagoValidos.Contains(metodoPago))
        {
            return "metodoPago solo admite 'tarjeta', 'yape' o 'contraentrega'.";
        }

        if (metodoPago == "tarjeta" && (numeroTarjeta is null || !Regex.IsMatch(numeroTarjeta, @"^\d{16}$")))
        {
            return "numeroTarjeta es obligatorio y debe tener 16 dígitos cuando metodoPago es 'tarjeta'.";
        }

        return null;
    }

    private static PedidoResponseDto MapearRespuesta(int idPedido, Pedido pedido, List<PedidoDetalle> detalle) => new()
    {
        IdPedido = idPedido,
        NumeroPedido = pedido.NumeroPedido,
        FechaPedido = pedido.FechaPedido,
        Total = pedido.Total,
        NombreCliente = pedido.NombreCliente,
        DireccionEnvio = pedido.DireccionEnvio,
        CiudadEnvio = pedido.CiudadEnvio,
        TelefonoContacto = pedido.TelefonoContacto,
        MetodoPago = pedido.MetodoPago,
        Items = detalle.Select(MapearItem).ToList(),
    };

    private static PedidoResponseDto MapearRespuesta(Pedido pedido, IEnumerable<PedidoDetalle> detalle) => new()
    {
        IdPedido = pedido.IdPedido,
        NumeroPedido = pedido.NumeroPedido,
        FechaPedido = pedido.FechaPedido,
        Total = pedido.Total,
        NombreCliente = pedido.NombreCliente,
        DireccionEnvio = pedido.DireccionEnvio,
        CiudadEnvio = pedido.CiudadEnvio,
        TelefonoContacto = pedido.TelefonoContacto,
        MetodoPago = pedido.MetodoPago,
        Items = detalle.Select(MapearItem).ToList(),
    };

    private static PedidoDetalleItemResponseDto MapearItem(PedidoDetalle d) => new()
    {
        IdProducto = d.IdProducto,
        NombreProducto = d.NombreProducto,
        PrecioUnitario = d.PrecioUnitario,
        Cantidad = d.Cantidad,
    };
}
