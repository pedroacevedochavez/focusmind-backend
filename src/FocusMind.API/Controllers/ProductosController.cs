using System.IdentityModel.Tokens.Jwt;
using FocusMind.Business.Services;
using FocusMind.DTO.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FocusMind.API.Controllers;

[ApiController]
[Route("api/productos")]
public sealed class ProductosController(IProductoService productoService) : ControllerBase
{
    // Pública (HU-15): catálogo y ficha técnica no requieren sesión.
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] ProductoFiltroDto filtro)
    {
        var productos = await productoService.ListarAsync(filtro);

        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var producto = await productoService.ObtenerDetalleAsync(id);
        if (producto is null)
        {
            return NotFound(new { error = "Producto no encontrado." });
        }

        return Ok(producto);
    }

    // NOTA (gap conocido, documentado en HU-15/HU-21): TM_USUARIO todavía no tiene columna de
    // rol, así que el JWT tampoco lleva claim de rol. [Authorize] aquí exige únicamente "estar
    // autenticado", NO "ser admin". Restringir por rol requiere un incremento futuro: agregar
    // ROL a TM_USUARIO, incluirlo como claim en JwtTokenGenerator, y usar
    // [Authorize(Roles = "admin")] en su lugar.
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Crear(ProductoCrearRequestDto request)
    {
        var resultado = await productoService.CrearAsync(request, ObtenerIdUsuarioActual());
        if (!resultado.Exito)
        {
            return UnprocessableEntity(new { error = resultado.MensajeError });
        }

        return CreatedAtAction(nameof(Obtener), new { id = resultado.Producto!.IdProducto }, resultado.Producto);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Actualizar(int id, ProductoActualizarRequestDto request)
    {
        var resultado = await productoService.ActualizarAsync(id, request, ObtenerIdUsuarioActual());
        if (!resultado.Exito)
        {
            return resultado.NoEncontrado
                ? NotFound(new { error = resultado.MensajeError })
                : UnprocessableEntity(new { error = resultado.MensajeError });
        }

        return Ok(resultado.Producto);
    }

    // Baja lógica (ACTIVO = 0), no DELETE físico: TM_PRODUCTO tiene FKs entrantes desde
    // TD_PEDIDO_DETALLE / TR_DIAGNOSTICO_RECOMENDACION (historial de compras/recomendaciones),
    // borrar la fila físicamente rompería esa integridad referencial.
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await productoService.EliminarAsync(id, ObtenerIdUsuarioActual());
        if (!eliminado)
        {
            return NotFound(new { error = "Producto no encontrado." });
        }

        return NoContent();
    }

    private int? ObtenerIdUsuarioActual()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(claim, out var idUsuario) ? idUsuario : null;
    }
}
