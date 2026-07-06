using FocusMind.DBContext.Repositories;
using FocusMind.DTO.Requests;
using FocusMind.DTO.Responses;

namespace FocusMind.Business.Services;

// HU-17: valida existencia + estado activo + stock suficiente de cada ítem del carrito ANTES
// de confirmar la compra (el carrito en sí sigue viviendo 100% en el Frontend — ver nota de
// ajuste de HU-17 en el informe, sin tabla de carrito persistente). No agrega SQL nuevo: se
// apoya íntegramente en IProductoRepository.ObtenerAsync, ya construido en HU-15.
public sealed class DisponibilidadService(IProductoRepository productoRepository) : IDisponibilidadService
{
    public async Task<ValidarDisponibilidadResponseDto> ValidarAsync(IEnumerable<ItemCarritoDto> items)
    {
        var resultado = new List<DisponibilidadItemResponseDto>();

        foreach (var item in items)
        {
            var producto = await productoRepository.ObtenerAsync(item.IdProducto);

            string? motivo = producto switch
            {
                null => "Producto no encontrado.",
                { Activo: false } => "El producto ya no está disponible.",
                _ when producto.Stock < item.Cantidad => $"Stock insuficiente (disponible: {producto.Stock}).",
                _ => null,
            };

            resultado.Add(new DisponibilidadItemResponseDto
            {
                IdProducto = item.IdProducto,
                CantidadSolicitada = item.Cantidad,
                Disponible = motivo is null,
                Motivo = motivo,
            });
        }

        return new ValidarDisponibilidadResponseDto
        {
            TodoDisponible = resultado.All(r => r.Disponible),
            Items = resultado,
        };
    }
}
