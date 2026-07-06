using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

public sealed class PedidoCrearRequestDto
{
    // Generado por la pasarela de pago simulada del Frontend (PaymentService.procesarPago,
    // HU-11: `TXN-${Date.now()}`), no por el backend — ver nota de alcance en PedidoService.
    // Si colisiona con uno existente, la API responde 409 (no 500).
    [Required]
    public string NumeroPedido { get; set; } = string.Empty;

    [Required, MinLength(3)]
    public string NombreCliente { get; set; } = string.Empty;

    [Required, MinLength(5)]
    public string DireccionEnvio { get; set; } = string.Empty;

    [Required]
    public string CiudadEnvio { get; set; } = string.Empty;

    [Required, RegularExpression(@"^9\d{8}$")]
    public string TelefonoContacto { get; set; } = string.Empty;

    [Required]
    public string MetodoPago { get; set; } = string.Empty;

    // Solo se usa para validar el formato cuando MetodoPago == "tarjeta" — NUNCA se persiste.
    // TM_PEDIDO no tiene columna para esto (cumplimiento PCI-DSS, ver notas de diseño del SQL).
    public string? NumeroTarjeta { get; set; }

    [Required, MinLength(1)]
    public List<ItemCarritoDto> Items { get; set; } = [];
}
