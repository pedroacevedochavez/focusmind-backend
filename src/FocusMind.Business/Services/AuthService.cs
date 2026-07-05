using System.IdentityModel.Tokens.Jwt;
using FocusMind.DBContext.Repositories;
using FocusMind.DBEntity;

namespace FocusMind.Business.Services;

public sealed class AuthService(IUsuarioRepository usuarioRepository, IJwtTokenGenerator tokenGenerator) : IAuthService
{
    // Coste de BCrypt (HU-14: hash con BCrypt.Net-Next). 12 es el estándar recomendado
    // actual (balance costo-cómputo/servidor vs. resistencia a fuerza bruta offline).
    private const int BCryptWorkFactor = 12;

    public async Task<ResultadoAutenticacion> RegistrarAsync(string nombre, string email, string password)
    {
        // NOTA: existe una ventana de condición de carrera entre esta verificación y el INSERT
        // (dos registros concurrentes con el mismo email). La restricción UNIQUE de TM_USUARIO.EMAIL
        // es el resguardo final: en ese caso el INSERT fallaría con una excepción de SQL no traducida
        // todavía a un mensaje de negocio — limitación conocida, no una omisión silenciosa.
        var existente = await usuarioRepository.ObtenerPorEmailAsync(email);
        if (existente is not null)
        {
            return new ResultadoAutenticacion(false, "El correo ya está registrado.", null, null, null);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, BCryptWorkFactor);
        var idUsuario = await usuarioRepository.InsertarAsync(nombre, email, passwordHash, usuarioCrea: null);

        var usuario = new Usuario { IdUsuario = idUsuario, Nombre = nombre, Email = email, Activo = true };
        var (accessToken, refreshToken) = tokenGenerator.Generar(usuario);

        return new ResultadoAutenticacion(true, null, usuario, accessToken, refreshToken);
    }

    public async Task<ResultadoAutenticacion> LoginAsync(string email, string password)
    {
        var usuario = await usuarioRepository.ObtenerPorEmailAsync(email);

        // Mensaje genérico en ambos casos (email inexistente o password incorrecta) para no
        // filtrar si un correo está registrado (evita enumeración de usuarios).
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(password, usuario.Password))
        {
            return new ResultadoAutenticacion(false, "Credenciales inválidas.", null, null, null);
        }

        var (accessToken, refreshToken) = tokenGenerator.Generar(usuario);

        return new ResultadoAutenticacion(true, null, usuario, accessToken, refreshToken);
    }

    public async Task<ResultadoAutenticacion> RefrescarTokenAsync(string refreshToken)
    {
        var principal = tokenGenerator.Validar(refreshToken);
        var idClaim = principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (idClaim is null || !int.TryParse(idClaim, out var idUsuario))
        {
            return new ResultadoAutenticacion(false, "Refresh token inválido o expirado.", null, null, null);
        }

        // Se revalida contra la BD (no solo contra los claims firmados) para detectar cuentas
        // desactivadas después de emitido el refresh token.
        var usuario = await usuarioRepository.ObtenerPorIdAsync(idUsuario);
        if (usuario is null || !usuario.Activo)
        {
            return new ResultadoAutenticacion(false, "Refresh token inválido o expirado.", null, null, null);
        }

        // Rotación: cada uso de un refresh token emite un par nuevo, reduciendo la ventana de replay.
        var (accessToken, nuevoRefreshToken) = tokenGenerator.Generar(usuario);

        return new ResultadoAutenticacion(true, null, usuario, accessToken, nuevoRefreshToken);
    }
}
