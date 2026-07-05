using FocusMind.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace FocusMind.API.Controllers;

// Contrato HU-13: GET /api/health -> 200 {status:'ok', db:'connected', timestamp}
// o 503 {status:'error', db:'disconnected'} sin exponer detalles de la excepción/cadena de conexión.
[ApiController]
[Route("api/health")]
public sealed class HealthController(IHealthService healthService, ILogger<HealthController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var conectado = await healthService.VerificarConexionAsync();
            if (!conectado)
            {
                throw new InvalidOperationException("SELECT 1 no devolvió el resultado esperado.");
            }

            return Ok(new
            {
                status = "ok",
                db = "connected",
                timestamp = DateTime.UtcNow.ToString("o")
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fallo de conectividad al verificar /api/health.");

            return StatusCode(503, new
            {
                status = "error",
                db = "disconnected"
            });
        }
    }
}
