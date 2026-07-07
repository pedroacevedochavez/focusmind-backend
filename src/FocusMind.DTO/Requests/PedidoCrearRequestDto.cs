using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

public sealed class PedidoCrearRequestDto
{
    // Generado por la pasarela de pago simulada del Frontend (PaymentService.procesarPago,
    // HU-11: `TXN-${Date.now()}`), no por el backend — ver nota de alcance en PedidoService.
    // Si colisiona con uno existente, la API responde 409 (no 500).
    // Límites de esta sección alineados a TM_PEDIDO (01_Schema_Tablas.sql) — HU-20.
    [Required, StringLength(50)]
    public string NumeroPedido { get; set; } = string.Empty;

    [Required, MinLength(3), StringLength(150)]
    public string NombreCliente { get; set; } = string.Empty;

    [Required, MinLength(5), StringLength(200)]
    public string DireccionEnvio { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string CiudadEnvio { get; set; } = string.Empty;

    [Required, RegularExpression(@"^9\d{8}$")]
    public string TelefonoContacto { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string MetodoPago { get; set; } = string.Empty;

    // Solo se usa para validar el formato cuando MetodoPago == "tarjeta" — NUNCA se persiste.
    // TM_PEDIDO no tiene columna para esto (cumplimiento PCI-DSS, ver notas de diseño del SQL).
    // Tope defensivo de 16 (ver validación exacta de formato en PedidoService).
    [StringLength(16)]
    public string? NumeroTarjeta { get; set; }

    // Tope defensivo (HU-20): mismo criterio que ValidarDisponibilidadRequestDto.Items.
    [Required, MinLength(1), MaxLength(50)]
    public List<ItemCarritoDto> Items { get; set; } = [];
}
