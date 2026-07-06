namespace FocusMind.DTO.Responses;

public sealed class DiagnosticoResponseDto
{
    // Null cuando el diagnóstico se calculó sin sesión activa (no persistido) — Persistido=false
    // lo confirma de forma explícita, sin que el cliente tenga que inferirlo del id ausente.
    public int? IdDiagnostico { get; set; }
    public DateTime Fecha { get; set; }
    public int NivelEstres { get; set; }
    public int CalidadSueno { get; set; }
    public string Objetivo { get; set; } = string.Empty;
    public int HorasConcentracion { get; set; }
    public string? CondicionMedica { get; set; }
    public List<string> Alergias { get; set; } = [];
    public List<ProductoRecomendadoResponseDto> Recomendaciones { get; set; } = [];
    public bool Persistido { get; set; }
}
