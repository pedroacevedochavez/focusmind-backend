namespace FocusMind.DBEntity;

public class Pedido
{
    public int IdPedido { get; set; }
    public int IdUsuario { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public DateTime FechaPedido { get; set; }
    public decimal Total { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string DireccionEnvio { get; set; } = string.Empty;
    public string CiudadEnvio { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
