using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public interface IObjetivoRepository
{
    Task<Objetivo?> ObtenerAsync(int idObjetivo);
}
