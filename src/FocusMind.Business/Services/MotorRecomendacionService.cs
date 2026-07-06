using FocusMind.DBContext.Repositories;
using FocusMind.DBEntity;

namespace FocusMind.Business.Services;

// Reproduce contra datos reales la misma lógica que ya usaba el Frontend Mock
// (ProductoService.obtenerRecomendaciones en el Angular original, HU-09): filtra por objetivo
// cognitivo y excluye productos cuyo alérgeno coincida con alguno declarado por el usuario.
public sealed class MotorRecomendacionService(IProductoRepository productoRepository) : IMotorRecomendacionService
{
    public async Task<IEnumerable<Producto>> RecomendarAsync(int idObjetivo, IEnumerable<int> alergiaIds, int limite = 3)
    {
        var alergiasDeclaradas = alergiaIds.ToHashSet();
        var productos = await productoRepository.ListarAsync();

        var candidatos = productos.Where(p => p.IdObjetivo == idObjetivo);

        var recomendados = new List<Producto>();

        // N+1 deliberado (mismo criterio documentado en HU-15/ProductoService.ListarAsync):
        // el catálogo es pequeño, no existe todavía un SP de alérgenos "por lote".
        foreach (var producto in candidatos)
        {
            if (recomendados.Count >= limite)
            {
                break;
            }

            var alergenosProducto = await productoRepository.ListarAlergenosAsync(producto.IdProducto);
            var tieneAlergenoExcluido = alergenosProducto.Any(a => alergiasDeclaradas.Contains(a.IdAlergeno));

            if (!tieneAlergenoExcluido)
            {
                recomendados.Add(producto);
            }
        }

        return recomendados;
    }
}
