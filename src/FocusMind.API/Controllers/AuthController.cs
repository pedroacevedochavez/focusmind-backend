using FocusMind.Business.Services;
using FocusMind.DTO.Requests;
using FocusMind.DTO.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FocusMind.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegistroRequestDto request)
    {
        var resultado = await authService.RegistrarAsync(request.Nombre, request.Email, request.Password);
        if (!resultado.Exito)
        {
            return Conflict(new { error = resultado.MensajeError });
        }

        return StatusCode(StatusCodes.Status201Created, MapearRespuesta(resultado));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var resultado = await authService.LoginAsync(request.Email, request.Password);
        if (!resultado.Exito)
        {
            // Mensaje genérico (no distingue "correo no existe" de "password incorrecta").
            return Unauthorized(new { error = "Credenciales inválidas." });
        }

        return Ok(MapearRespuesta(resultado));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequestDto request)
    {
        var resultado = await authService.RefrescarTokenAsync(request.RefreshToken);
        if (!resultado.Exito)
        {
            return Unauthorized(new { error = resultado.MensajeError });
        }

        return Ok(MapearRespuesta(resultado));
    }

    private static AuthResponseDto MapearRespuesta(ResultadoAutenticacion resultado) => new()
    {
        Usuario = new UsuarioResponseDto
        {
            IdUsuario = resultado.Usuario!.IdUsuario,
            Nombre = resultado.Usuario.Nombre,
            Email = resultado.Usuario.Email,
        },
        AccessToken = resultado.AccessToken!,
        RefreshToken = resultado.RefreshToken!,
    };
}
