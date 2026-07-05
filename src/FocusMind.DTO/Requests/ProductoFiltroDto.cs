namespace FocusMind.DTO.Requests;

// Query params de GET /api/productos: ?categoria=&objetivo=&precioMax=&q=
public sealed class ProductoFiltroDto
{
    public int? Categoria { get; set; }
    public int? Objetivo { get; set; }
    public decimal? PrecioMax { get; set; }
    public string? Q { get; set; }
}
