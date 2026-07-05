using FocusMind.DBContext.Repositories;

namespace FocusMind.Business.Services;

public sealed class HealthService(IHealthRepository healthRepository) : IHealthService
{
    public Task<bool> VerificarConexionAsync() => healthRepository.VerificarConexionAsync();
}
