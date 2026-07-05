using FocusMind.DBEntity;
using FocusMind.DTO.Requests;
using FocusMind.DTO.Responses;

namespace FocusMind.Business.Services;

public sealed record ResultadoProducto(bool Exito, string? MensajeError, bool NoEncontrado, ProductoDetalleResponseDto? Producto)
{
    public static ResultadoProducto Ok(ProductoDetalleResponseDto producto) => new(true, null, false, producto);

    public static ResultadoProducto ErrorValidacion(string mensaje) => new(false, mensaje, false, null);

    public static ResultadoProducto NoExiste(string mensaje) => new(false, mensaje, true, null);
}

public interface IProductoService
{
    Task<IEnumerable<Producto>> ObtenerMuestraAsync(int cantidad = 5);

    Task<IEnumerable<ProductoListItemResponseDto>> ListarAsync(ProductoFiltroDto filtro);

    Task<ProductoDetalleResponseDto?> ObtenerDetalleAsync(int idProducto);

    Task<ResultadoProducto> CrearAsync(ProductoCrearRequestDto dto, int? usuarioCrea);

    Task<ResultadoProducto> ActualizarAsync(int idProducto, ProductoActualizarRequestDto dto, int? usuarioModifica);

    Task<bool> EliminarAsync(int idProducto, int? usuarioModifica);
}
