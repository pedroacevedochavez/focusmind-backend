using FocusMind.DBEntity;

namespace FocusMind.Business.Services;

public sealed record ResultadoAutenticacion(
    bool Exito,
    string? MensajeError,
    Usuario? Usuario,
    string? AccessToken,
    string? RefreshToken);

public interface IAuthService
{
    Task<ResultadoAutenticacion> RegistrarAsync(string nombre, string email, string password);

    Task<ResultadoAutenticacion> LoginAsync(string email, string password);

    Task<ResultadoAutenticacion> RefrescarTokenAsync(string refreshToken);
}
