using System.IdentityModel.Tokens.Jwt;
using FocusMind.Business.Services;
using FocusMind.DTO.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FocusMind.API.Controllers;

[ApiController]
[Route("api/diagnosticos")]
public sealed class DiagnosticosController(IDiagnosticoService diagnosticoService) : ControllerBase
{
    // Público y dinámico (regla de negocio HU-16): a propósito SIN [Authorize]. El middleware
    // UseAuthentication() ya corre en toda request del pipeline (Program.cs), así que si llega
    // un Bearer token válido, User queda autenticado igual sin necesitar exigirlo con
    // [Authorize]. Si el token es inválido/expiró, el handler de JwtBearer simplemente deja a
    // User sin autenticar (no lanza 401) porque este endpoint no lo exige — el visitante
    // anónimo nunca ve un error por esto, solo pierde la persistencia.
    [HttpPost]
    public async Task<IActionResult> Crear(DiagnosticoCrearRequestDto request)
    {
        var idUsuario = ObtenerIdUsuarioActual();
        var resultado = await diagnosticoService.ProcesarAsync(request, idUsuario);

        // 201 si quedó guardado en RDS, 200 si fue puramente en memoria (sin sesión) —
        // el campo Persistido en el body ya lo confirma explícitamente para el cliente.
        return StatusCode(
            resultado.Persistido ? StatusCodes.Status201Created : StatusCodes.Status200OK,
            resultado);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar()
    {
        var idUsuario = ObtenerIdUsuarioActual()
            ?? throw new InvalidOperationException("Token autenticado sin claim de usuario válido.");

        var diagnosticos = await diagnosticoService.ListarXUsuarioAsync(idUsuario);

        return Ok(diagnosticos);
    }

    [HttpGet("ultimo")]
    [Authorize]
    public async Task<IActionResult> ObtenerUltimo()
    {
        var idUsuario = ObtenerIdUsuarioActual()
            ?? throw new InvalidOperationException("Token autenticado sin claim de usuario válido.");

        var ultimo = await diagnosticoService.ObtenerUltimoAsync(idUsuario);
        if (ultimo is null)
        {
            return NotFound(new { error = "Sin diagnósticos registrados" });
        }

        return Ok(ultimo);
    }

    private int? ObtenerIdUsuarioActual()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(claim, out var idUsuario) ? idUsuario : null;
    }
}
