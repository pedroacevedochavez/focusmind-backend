using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

public sealed class ValidarDisponibilidadRequestDto
{
    // Tope defensivo (HU-20): ningún carrito legítimo de este catálogo (8 productos) necesita
    // más de unas pocas decenas de líneas.
    [Required, MinLength(1), MaxLength(50)]
    public List<ItemCarritoDto> Items { get; set; } = [];
}
