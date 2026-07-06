namespace FocusMind.DTO.Responses;

public sealed class PerfilCognitivoResponseDto
{
    public int NivelEstres { get; set; }
    public int CalidadSueno { get; set; }
    public string ObjetivoPrincipal { get; set; } = string.Empty;
}
