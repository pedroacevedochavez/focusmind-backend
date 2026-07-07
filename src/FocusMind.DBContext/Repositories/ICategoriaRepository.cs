using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public interface ICategoriaRepository
{
    Task<IEnumerable<Categoria>> ListarAsync();
}
