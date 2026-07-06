namespace FocusMind.DTO.Responses;

public sealed class PedidoDetalleItemResponseDto
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal => PrecioUnitario * Cantidad;
}
