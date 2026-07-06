using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public interface IPedidoRepository
{
    Task<IEnumerable<Pedido>> ListarXUsuarioAsync(int idUsuario);

    Task<Pedido?> ObtenerAsync(int idPedido);

    Task<IEnumerable<PedidoDetalle>> ListarDetalleAsync(int idPedido);

    // Lanza StockInsuficienteException o NumeroPedidoDuplicadoException si la transacción falla.
    Task<int> ConfirmarAsync(Pedido pedido, IEnumerable<PedidoDetalle> detalle, int? usuarioCrea);
}
