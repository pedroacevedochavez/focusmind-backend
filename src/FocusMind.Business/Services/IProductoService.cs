using FocusMind.DBEntity;

namespace FocusMind.Business.Services;

public interface IProductoService
{
    Task<IEnumerable<Producto>> ObtenerMuestraAsync(int cantidad = 5);
}
