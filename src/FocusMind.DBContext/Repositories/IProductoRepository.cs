using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public interface IProductoRepository
{
    Task<IEnumerable<Producto>> ListarAsync();
}
