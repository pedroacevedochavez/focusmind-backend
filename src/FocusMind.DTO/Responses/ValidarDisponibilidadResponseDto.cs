namespace FocusMind.DTO.Responses;

public sealed class ValidarDisponibilidadResponseDto
{
    public bool TodoDisponible { get; set; }
    public List<DisponibilidadItemResponseDto> Items { get; set; } = [];
}
