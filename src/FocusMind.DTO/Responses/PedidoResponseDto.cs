namespace FocusMind.DTO.Responses;

public sealed class PedidoResponseDto
{
    public int IdPedido { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public DateTime FechaPedido { get; set; }
    public decimal Total { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string DireccionEnvio { get; set; } = string.Empty;
    public string CiudadEnvio { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public List<PedidoDetalleItemResponseDto> Items { get; set; } = [];
}
