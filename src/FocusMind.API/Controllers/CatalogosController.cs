using FocusMind.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace FocusMind.API.Controllers;

// Público (igual que el catálogo de productos): el Frontend los usa para resolver sus slugs
// estáticos (Categoria/ObjetivoCognitivo) al id que espera ProductoFiltroDto, sin necesitar sesión.
[ApiController]
[Route("api")]
public sealed class CatalogosController(ICatalogoService catalogoService) : ControllerBase
{
    [HttpGet("categorias")]
    public async Task<IActionResult> ListarCategorias()
    {
        var categorias = await catalogoService.ListarCategoriasAsync();

        return Ok(categorias);
    }

    [HttpGet("objetivos")]
    public async Task<IActionResult> ListarObjetivos()
    {
        var objetivos = await catalogoService.ListarObjetivosAsync();

        return Ok(objetivos);
    }

    [HttpGet("alergenos")]
    public async Task<IActionResult> ListarAlergenos()
    {
        var alergenos = await catalogoService.ListarAlergenosAsync();

        return Ok(alergenos);
    }
}
