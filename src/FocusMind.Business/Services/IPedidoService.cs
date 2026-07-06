using FocusMind.DTO.Requests;
using FocusMind.DTO.Responses;

namespace FocusMind.Business.Services;

public sealed record ResultadoPedido(bool Exito, string? MensajeError, bool Conflicto, PedidoResponseDto? Pedido)
{
    public static ResultadoPedido Ok(PedidoResponseDto pedido) => new(true, null, false, pedido);

    public static ResultadoPedido ErrorValidacion(string mensaje) => new(false, mensaje, false, null);

    public static ResultadoPedido ErrorConflicto(string mensaje) => new(false, mensaje, true, null);
}

public interface IPedidoService
{
    Task<ResultadoPedido> ConfirmarPedidoAsync(PedidoCrearRequestDto dto, int idUsuario);

    Task<IEnumerable<PedidoListItemResponseDto>> ListarXUsuarioAsync(int idUsuario);

    // Devuelve null si el pedido no existe O si pertenece a otro usuario (ver PedidoService).
    Task<PedidoResponseDto?> ObtenerAsync(int idPedido, int idUsuarioSolicitante);
}
