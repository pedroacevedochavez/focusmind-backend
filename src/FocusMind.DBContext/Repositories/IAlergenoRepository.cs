using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public interface IAlergenoRepository
{
    Task<IEnumerable<Alergeno>> ListarAsync();
}
