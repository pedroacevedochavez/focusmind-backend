namespace FocusMind.DBEntity;

public class PedidoDetalle
{
    public int IdPedidoDetalle { get; set; }
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
}
