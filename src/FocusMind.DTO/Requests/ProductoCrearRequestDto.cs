using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

public sealed class ProductoCrearRequestDto
{
    [Required, MinLength(2)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MinLength(2)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    public int IdCategoria { get; set; }

    [Required]
    public int IdObjetivo { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Precio { get; set; }

    [Required]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public string DosisRecomendada { get; set; } = string.Empty;

    // URL ya generada por el cliente contra Amazon S3 (HU-22) — la API nunca recibe el binario.
    public string? UrlImagen { get; set; }

    public string? RegistroSanitario { get; set; }

    public string? EntidadRegistro { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public List<string> Ingredientes { get; set; } = [];

    public List<string> Contraindicaciones { get; set; } = [];

    // IDs de TM_ALERGENO ya existentes (catálogo), no texto libre.
    public List<int> AlergenoIds { get; set; } = [];
}
