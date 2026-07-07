using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

// Query params de GET /api/productos: ?categoria=&objetivo=&precioMax=&q=
public sealed class ProductoFiltroDto
{
    public int? Categoria { get; set; }
    public int? Objetivo { get; set; }
    public decimal? PrecioMax { get; set; }

    // HU-20: tope defensivo — es un filtro de búsqueda en memoria (ver ProductoService.ListarAsync),
    // no una cláusula SQL LIKE, pero un query string absurdamente largo no aporta nada legítimo.
    [StringLength(200)]
    public string? Q { get; set; }
}
