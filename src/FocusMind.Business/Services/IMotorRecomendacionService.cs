using FocusMind.DBEntity;

namespace FocusMind.Business.Services;

public interface IMotorRecomendacionService
{
    Task<IEnumerable<Producto>> RecomendarAsync(int idObjetivo, IEnumerable<int> alergiaIds, int limite = 3);
}
