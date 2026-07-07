using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

// HU-20: los StringLength de este DTO son un espejo exacto de TM_PRODUCTO (01_Schema_Tablas.sql)
// para que un valor demasiado largo falle como 400 aquí en vez de como un 500 de SQL al
// truncar/rechazar en el INSERT. Los MaxLength de las listas son un tope propio (no hay
// columna equivalente): evita que un solo request intente insertar cientos de filas en
// TD_PRODUCTO_INGREDIENTE/TD_PRODUCTO_CONTRAINDICACION/TR_PRODUCTO_ALERGENO.
public sealed class ProductoCrearRequestDto : IValidatableObject
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

    // URL ya generada por el cliente contra Amazon S3 (HU-22) — la API nunca recibe el binario.
    [StringLength(500)]
    public string? UrlImagen { get; set; }

    [StringLength(50)]
    public string? RegistroSanitario { get; set; }

    [StringLength(10)]
    public string? EntidadRegistro { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

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
