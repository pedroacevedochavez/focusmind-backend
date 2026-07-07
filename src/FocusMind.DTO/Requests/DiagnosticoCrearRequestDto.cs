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

    // Espeja TM_DIAGNOSTICO.CONDICIONMEDICA VARCHAR(300).
    [StringLength(300)]
    public string? CondicionMedica { get; set; }

    // IDs de TM_ALERGENO (catálogo) — mismo criterio que ProductoCrearRequestDto.AlergenoIds
    // en HU-15 (por ID, no texto libre). Tope defensivo: el catálogo real tiene un puñado de
    // alérgenos, no hay motivo legítimo para enviar más de unas pocas decenas de IDs.
    [MaxLength(50)]
    public List<int> AlergiaIds { get; set; } = [];
}
