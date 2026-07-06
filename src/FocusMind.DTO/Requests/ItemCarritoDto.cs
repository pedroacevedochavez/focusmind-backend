using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

// Espeja ItemCarrito del Frontend (models/pedido/pedido.ts): el carrito vive 100% en Angular
// (Signals, sin tabla de carrito persistente — ver nota de ajuste de HU-17 en el informe);
// esto es solo el contrato de transporte para validar/confirmar contra el backend.
public sealed class ItemCarritoDto
{
    public int IdProducto { get; set; }

    [Range(1, 10)]
    public int Cantidad { get; set; }
}
