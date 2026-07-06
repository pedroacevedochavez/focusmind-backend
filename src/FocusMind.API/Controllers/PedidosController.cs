using FocusMind.Business.Services;
using FocusMind.DTO.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FocusMind.API.Controllers;

// NOTA DE ALCANCE (HU-17 vs HU-18): este controller hoy SOLO valida disponibilidad — no crea
// pedidos. La creación real (POST /api/pedidos, transacción ACID con usp_InsertarPedido +
// usp_InsertarPedidoDetalle, descuento de stock) es HU-18 y todavía no está implementada.
// Cuando se construya, PedidoService.ConfirmarPedido debe invocar el mismo
// IDisponibilidadService usado aquí ANTES de abrir su propia transacción — así lo pide
// explícitamente el informe ("comparte transacción con HU-18") y evita duplicar la regla de
// negocio de disponibilidad en dos lugares.
[ApiController]
[Route("api/pedidos")]
public sealed class PedidosController(IDisponibilidadService disponibilidadService) : ControllerBase
{
    // [Authorize]: el checkout es una operación transaccional crítica y en el Frontend la ruta
    // /checkout ya está detrás de authGuard (HU-11) — solo un usuario autenticado llega a este paso.
    [HttpPost("disponibilidad")]
    [Authorize]
    public async Task<IActionResult> ValidarDisponibilidad(ValidarDisponibilidadRequestDto request)
    {
        var resultado = await disponibilidadService.ValidarAsync(request.Items);

        // 200 si todo el carrito está disponible, 400 si algún ítem tiene un problema —
        // el detalle por ítem (Motivo) va siempre en el body, disponible o no, para que el
        // Frontend pueda resaltar exactamente qué línea del carrito falló.
        return resultado.TodoDisponible ? Ok(resultado) : BadRequest(resultado);
    }
}
