namespace FocusMind.DBEntity;

// Forma slim de Producto devuelta por usp_Listar_DiagnosticoRecomendacion_X_Diagnostico
// (IDPRODUCTO, NOMBRE, MARCA, PRECIO, URLIMAGEN, STOCK) — no el Producto completo, para no
// insinuar que trae Descripcion/Categoria/etc. cuando en realidad esas columnas nunca se piden.
public class ProductoRecomendado
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string? UrlImagen { get; set; }
    public int Stock { get; set; }
}
