using System.IdentityModel.Tokens.Jwt;
using FocusMind.Business.Services;
using FocusMind.DTO.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FocusMind.API.Controllers;

// HU-17 (disponibilidad) + HU-18 (confirmación de pedido con transacción ACID).
[ApiController]
[Route("api/pedidos")]
public sealed class PedidosController(IDisponibilidadService disponibilidadService, IPedidoService pedidoService) : ControllerBase
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

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Confirmar(PedidoCrearRequestDto request)
    {
        var idUsuario = ObtenerIdUsuarioActual()
            ?? throw new InvalidOperationException("Token autenticado sin claim de usuario válido.");

        var resultado = await pedidoService.ConfirmarPedidoAsync(request, idUsuario);
        if (!resultado.Exito)
        {
            return resultado.Conflicto
                ? Conflict(new { error = resultado.MensajeError })
                : BadRequest(new { error = resultado.MensajeError });
        }

        return CreatedAtAction(nameof(Obtener), new { id = resultado.Pedido!.IdPedido }, resultado.Pedido);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar()
    {
        var idUsuario = ObtenerIdUsuarioActual()
            ?? throw new InvalidOperationException("Token autenticado sin claim de usuario válido.");

        var pedidos = await pedidoService.ListarXUsuarioAsync(idUsuario);

        return Ok(pedidos);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Obtener(int id)
    {
        var idUsuario = ObtenerIdUsuarioActual()
            ?? throw new InvalidOperationException("Token autenticado sin claim de usuario válido.");

        var pedido = await pedidoService.ObtenerAsync(id, idUsuario);
        if (pedido is null)
        {
            return NotFound(new { error = "Pedido no encontrado." });
        }

        return Ok(pedido);
    }

    private int? ObtenerIdUsuarioActual()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(claim, out var idUsuario) ? idUsuario : null;
    }
}
