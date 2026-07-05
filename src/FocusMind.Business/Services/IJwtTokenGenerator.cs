using System.Security.Claims;
using FocusMind.DBEntity;

namespace FocusMind.Business.Services;

// Configuración de firma/expiración de tokens. En dev se puebla desde appsettings.json
// (Jwt:Key/Issuer/Audience/...); en producción la clave debe venir de AWS Secrets Manager
// (ver HU-20/HU-22), nunca hardcodeada ni versionada en el repositorio.
public sealed record JwtOptions(string Key, string Issuer, string Audience, int AccessTokenMinutes, int RefreshTokenDays);

public interface IJwtTokenGenerator
{
    (string AccessToken, string RefreshToken) Generar(Usuario usuario);

    ClaimsPrincipal? Validar(string token);
}
