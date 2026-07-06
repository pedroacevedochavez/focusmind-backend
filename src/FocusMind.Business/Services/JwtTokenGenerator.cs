using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FocusMind.DBEntity;
using Microsoft.IdentityModel.Tokens;

namespace FocusMind.Business.Services;

public sealed class JwtTokenGenerator(JwtOptions opciones) : IJwtTokenGenerator
{
    public (string AccessToken, string RefreshToken) Generar(Usuario usuario)
    {
        var accessToken = GenerarToken(usuario, TimeSpan.FromMinutes(opciones.AccessTokenMinutes));
        var refreshToken = GenerarToken(usuario, TimeSpan.FromDays(opciones.RefreshTokenDays));

        return (accessToken, refreshToken);
    }

    // Validación local (mismo firmante que emitió el token). No existe todavía una tabla de
    // revocación de refresh tokens (queda documentado como pendiente en el informe, HU-14):
    // un token robado sigue siendo válido hasta su expiración natural. Si se necesita logout
    // con invalidación real, el siguiente incremento debe agregar esa tabla y consultarla aquí.
    public ClaimsPrincipal? Validar(string token)
    {
        var parametros = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = opciones.Issuer,
            ValidateAudience = true,
            ValidAudience = opciones.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opciones.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            // MapInboundClaims = false: evita que "sub"/"email" se re-mapeen a URIs legacy de
            // .NET (ClaimTypes.NameIdentifier/Email) — mismo motivo que options.MapInboundClaims
            // en el AddJwtBearer de Program.cs, pero aquí aplica al handler usado manualmente.
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

            return handler.ValidateToken(token, parametros, out _);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }

    private string GenerarToken(Usuario usuario, TimeSpan vigencia)
    {
        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opciones.Key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: opciones.Issuer,
            audience: opciones.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(vigencia),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
