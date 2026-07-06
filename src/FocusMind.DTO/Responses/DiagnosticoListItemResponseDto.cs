namespace FocusMind.DTO.Responses;

public sealed class DiagnosticoListItemResponseDto
{
    public int IdDiagnostico { get; set; }
    public DateTime Fecha { get; set; }
    public int NivelEstres { get; set; }
    public int CalidadSueno { get; set; }
    public string Objetivo { get; set; } = string.Empty;
    public int HorasConcentracion { get; set; }
    public string? CondicionMedica { get; set; }
}
