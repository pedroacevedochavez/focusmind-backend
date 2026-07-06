using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

public sealed class DiagnosticoCrearRequestDto
{
    [Range(1, 10)]
    public int NivelEstres { get; set; }

    [Range(1, 10)]
    public int CalidadSueno { get; set; }

    [Required]
    public int IdObjetivo { get; set; }

    [Range(1, 16)]
    public int HorasConcentracion { get; set; }

    public string? CondicionMedica { get; set; }

    // IDs de TM_ALERGENO (catálogo) — mismo criterio que ProductoCrearRequestDto.AlergenoIds
    // en HU-15 (por ID, no texto libre).
    public List<int> AlergiaIds { get; set; } = [];
}
