using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

// HU-21 — Transparencia sanitaria persistente: hasta HU-15/HU-20 este DTO no incluía
// Ingredientes/Contraindicaciones/AlergenoIds, así que un PUT nunca podía corregir esos datos
// sanitarios (solo quedaban fijos desde la creación del producto). Ahora se administran igual
// que en ProductoCrearRequestDto — misma validación, mismos límites — y ProductoService las
// resincroniza en la misma transacción del UPDATE (ver ProductoRepository.ActualizarAsync).
public sealed class ProductoActualizarRequestDto : IValidatableObject
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

    [MaxLength(50)]
    public List<string> Ingredientes { get; set; } = [];

    [MaxLength(50)]
    public List<string> Contraindicaciones { get; set; } = [];

    // IDs de TM_ALERGENO ya existentes (catálogo), no texto libre.
    [MaxLength(50)]
    public List<int> AlergenoIds { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        ProductoListasSanitariasValidacion.Validar(
            Ingredientes, Contraindicaciones, nameof(Ingredientes), nameof(Contraindicaciones));
}
