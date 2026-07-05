using FocusMind.DBContext.Repositories;
using FocusMind.DBEntity;

namespace FocusMind.Business.Services;

public sealed class ProductoService(IProductoRepository productoRepository) : IProductoService
{
    public async Task<IEnumerable<Producto>> ObtenerMuestraAsync(int cantidad = 5)
    {
        var productos = await productoRepository.ListarAsync();

        return productos.Take(cantidad);
    }
}
