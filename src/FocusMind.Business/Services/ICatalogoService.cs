using FocusMind.DTO.Responses;

namespace FocusMind.Business.Services;

// HU-19: el Frontend necesita resolver sus slugs estáticos (Categoria/ObjetivoCognitivo,
// ej. 'memoria', 'mejorar-memoria') al IDCATEGORIA/IDOBJETIVO que espera ProductoFiltroDto,
// y viceversa (mostrar el nombre legible que ya devuelve ProductoListItemResponseDto). Estos
// dos SPs (usp_ListarCategoria/usp_ListarObjetivo) ya existían desde el diseño original del
// esquema pero nunca se habían expuesto vía API — no requiere ninguna migración de SQL nueva.
public interface ICatalogoService
{
    Task<IEnumerable<CategoriaResponseDto>> ListarCategoriasAsync();

    Task<IEnumerable<ObjetivoResponseDto>> ListarObjetivosAsync();

    Task<IEnumerable<AlergenoResponseDto>> ListarAlergenosAsync();
}
