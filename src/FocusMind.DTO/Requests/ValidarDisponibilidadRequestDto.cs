using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

public sealed class ValidarDisponibilidadRequestDto
{
    [Required, MinLength(1)]
    public List<ItemCarritoDto> Items { get; set; } = [];
}
