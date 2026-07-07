using FocusMind.DBContext.Repositories;
using FocusMind.DTO.Responses;

namespace FocusMind.Business.Services;

public sealed class CatalogoService(
    ICategoriaRepository categoriaRepository,
    IObjetivoRepository objetivoRepository,
    IAlergenoRepository alergenoRepository) : ICatalogoService
{
    public async Task<IEnumerable<CategoriaResponseDto>> ListarCategoriasAsync()
    {
        var categorias = await categoriaRepository.ListarAsync();

        return categorias.Select(c => new CategoriaResponseDto
        {
            IdCategoria = c.IdCategoria,
            Codigo = c.Codigo,
            Nombre = c.Nombre,
        });
    }

    public async Task<IEnumerable<ObjetivoResponseDto>> ListarObjetivosAsync()
    {
        var objetivos = await objetivoRepository.ListarAsync();

        return objetivos.Select(o => new ObjetivoResponseDto
        {
            IdObjetivo = o.IdObjetivo,
            Codigo = o.Codigo,
            Nombre = o.Nombre,
        });
    }

    public async Task<IEnumerable<AlergenoResponseDto>> ListarAlergenosAsync()
    {
        var alergenos = await alergenoRepository.ListarAsync();

        return alergenos.Select(a => new AlergenoResponseDto
        {
            IdAlergeno = a.IdAlergeno,
            Nombre = a.Nombre,
        });
    }
}
