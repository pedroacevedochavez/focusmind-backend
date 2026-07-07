using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

// No incluye ingredientes/contraindicaciones/alérgenos: administrar esas colecciones
// queda fuera de alcance de esta historia (ver nota en ProductoService.ActualizarAsync).
public sealed class ProductoActualizarRequestDto
{
    [Required, MinLength(2), StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MinLength(2), StringLength(100)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    public int IdCategoria { get; set; }

    [Required]
    public int IdObjetivo { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Precio { get; set; }

    [Required, StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string DosisRecomendada { get; set; } = string.Empty;

    [StringLength(500)]
    public string? UrlImagen { get; set; }

    [StringLength(50)]
    public string? RegistroSanitario { get; set; }

    [StringLength(10)]
    public string? EntidadRegistro { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public bool Activo { get; set; } = true;
}
