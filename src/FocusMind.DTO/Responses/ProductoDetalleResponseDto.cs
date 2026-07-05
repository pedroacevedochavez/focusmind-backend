namespace FocusMind.DTO.Responses;

public sealed class ProductoDetalleResponseDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Objetivo { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string DosisRecomendada { get; set; } = string.Empty;
    public string? UrlImagen { get; set; }
    public string? RegistroSanitario { get; set; }
    public string? EntidadRegistro { get; set; }
    public int Stock { get; set; }
    public List<string> Ingredientes { get; set; } = [];
    public List<string> Contraindicaciones { get; set; } = [];
    public List<string> Alergenos { get; set; } = [];
}
