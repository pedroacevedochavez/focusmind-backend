namespace FocusMind.Business.Services;

public interface IHealthService
{
    Task<bool> VerificarConexionAsync();
}
