namespace FocusMind.DTO.Responses;

public sealed class ProductoRecomendadoResponseDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string? UrlImagen { get; set; }
    public int Stock { get; set; }
}
