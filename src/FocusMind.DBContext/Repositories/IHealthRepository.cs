namespace FocusMind.DBContext.Repositories;

public interface IHealthRepository
{
    Task<bool> VerificarConexionAsync();
}
