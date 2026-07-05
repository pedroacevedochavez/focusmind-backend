using FocusMind.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace FocusMind.API.Controllers;

// Prueba de humo de conectividad hacia SQL Server/RDS. No representa un endpoint
// de negocio real — solo confirma que API -> Business -> DBContext -> SQL Server
// está correctamente enganchado de punta a punta.
[ApiController]
[Route("api/[controller]")]
public sealed class ConnectionTestController(IProductoService productoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var productos = await productoService.ObtenerMuestraAsync(5);

            return Ok(new
            {
                mensaje = "Conexión a base de datos exitosa",
                productos
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                mensaje = "Error al conectar con la base de datos",
                tipo = ex.GetType().Name,
                error = ex.Message
            });
        }
    }
}
