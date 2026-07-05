namespace FocusMind.DBEntity;

public class Producto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public int IdCategoria { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int IdObjetivo { get; set; }
    public string Objetivo { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string DosisRecomendada { get; set; } = string.Empty;
    public string? UrlImagen { get; set; }
    public string? RegistroSanitario { get; set; }
    public string? EntidadRegistro { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; }
}
