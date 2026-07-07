using FocusMind.DBContext.Repositories;
using FocusMind.DBEntity;
using FocusMind.DTO.Requests;
using FocusMind.DTO.Responses;

namespace FocusMind.Business.Services;

public sealed class ProductoService(IProductoRepository productoRepository) : IProductoService
{
    public async Task<IEnumerable<ProductoListItemResponseDto>> ListarAsync(ProductoFiltroDto filtro)
    {
        var productos = await productoRepository.ListarAsync();

        var filtrados = productos
            .Where(p => filtro.Categoria is null || p.IdCategoria == filtro.Categoria)
            .Where(p => filtro.Objetivo is null || p.IdObjetivo == filtro.Objetivo)
            .Where(p => filtro.PrecioMax is null || p.Precio <= filtro.PrecioMax)
            .Where(p => string.IsNullOrWhiteSpace(filtro.Q) ||
                        p.Nombre.Contains(filtro.Q, StringComparison.OrdinalIgnoreCase) ||
                        p.Marca.Contains(filtro.Q, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var resultado = new List<ProductoListItemResponseDto>(filtrados.Count);

        // N+1 deliberado: el catálogo sembrado es de 8 productos; no existe (ni se justifica
        // todavía) un procedimiento de alérgenos "por lote" para N productos a la vez.
        foreach (var producto in filtrados)
        {
            var alergenos = await productoRepository.ListarAlergenosAsync(producto.IdProducto);

            resultado.Add(new ProductoListItemResponseDto
            {
                IdProducto = producto.IdProducto,
                Nombre = producto.Nombre,
                Marca = producto.Marca,
                Categoria = producto.Categoria,
                Objetivo = producto.Objetivo,
                Precio = producto.Precio,
                UrlImagen = producto.UrlImagen,
                RegistroSanitario = producto.RegistroSanitario,
                EntidadRegistro = producto.EntidadRegistro,
                Stock = producto.Stock,
                Alergenos = alergenos.Select(a => a.Nombre).ToList(),
            });
        }

        return resultado;
    }

    public async Task<ProductoDetalleResponseDto?> ObtenerDetalleAsync(int idProducto)
    {
        var producto = await productoRepository.ObtenerAsync(idProducto);
        if (producto is null)
        {
            return null;
        }

        var ingredientes = await productoRepository.ListarIngredientesAsync(idProducto);
        var contraindicaciones = await productoRepository.ListarContraindicacionesAsync(idProducto);
        var alergenos = await productoRepository.ListarAlergenosAsync(idProducto);

        return MapearDetalle(producto, ingredientes, contraindicaciones, alergenos);
    }

    public async Task<ResultadoProducto> CrearAsync(ProductoCrearRequestDto dto, int? usuarioCrea)
    {
        var errorAbet2 = ValidarRegistroSanitario(dto.RegistroSanitario, dto.EntidadRegistro);
        if (errorAbet2 is not null)
        {
            return ResultadoProducto.ErrorValidacion(errorAbet2);
        }

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Marca = dto.Marca,
            IdCategoria = dto.IdCategoria,
            IdObjetivo = dto.IdObjetivo,
            Precio = dto.Precio,
            Descripcion = dto.Descripcion,
            DosisRecomendada = dto.DosisRecomendada,
            UrlImagen = dto.UrlImagen,
            RegistroSanitario = dto.RegistroSanitario,
            EntidadRegistro = dto.EntidadRegistro,
            Stock = dto.Stock,
        };

        var idProducto = await productoRepository.InsertarAsync(
            producto, dto.Ingredientes, dto.Contraindicaciones, dto.AlergenoIds, usuarioCrea);

        var detalle = await ObtenerDetalleAsync(idProducto);

        return ResultadoProducto.Ok(detalle!);
    }

    // NOTA DE ALCANCE: no administra ingredientes/contraindicaciones/alérgenos (ver
    // ProductoActualizarRequestDto) — gestionar esas colecciones queda para un incremento
    // futuro (sub-recursos propios, ej. PUT /api/productos/:id/ingredientes).
    public async Task<ResultadoProducto> ActualizarAsync(int idProducto, ProductoActualizarRequestDto dto, int? usuarioModifica)
    {
        var errorAbet2 = ValidarRegistroSanitario(dto.RegistroSanitario, dto.EntidadRegistro);
        if (errorAbet2 is not null)
        {
            return ResultadoProducto.ErrorValidacion(errorAbet2);
        }

        var producto = new Producto
        {
            IdProducto = idProducto,
            Nombre = dto.Nombre,
            Marca = dto.Marca,
            IdCategoria = dto.IdCategoria,
            IdObjetivo = dto.IdObjetivo,
            Precio = dto.Precio,
            Descripcion = dto.Descripcion,
            DosisRecomendada = dto.DosisRecomendada,
            UrlImagen = dto.UrlImagen,
            RegistroSanitario = dto.RegistroSanitario,
            EntidadRegistro = dto.EntidadRegistro,
            Stock = dto.Stock,
            Activo = dto.Activo,
        };

        var actualizado = await productoRepository.ActualizarAsync(producto, usuarioModifica);
        if (!actualizado)
        {
            return ResultadoProducto.NoExiste("No existe ningún producto con el id especificado.");
        }

        var detalle = await ObtenerDetalleAsync(idProducto);

        return ResultadoProducto.Ok(detalle!);
    }

    public Task<bool> EliminarAsync(int idProducto, int? usuarioModifica) =>
        productoRepository.EliminarAsync(idProducto, usuarioModifica);

    // Replica en Business la restricción CHECK de TM_PRODUCTO (pareidad registroSanitario/
    // entidadRegistro + dominio cerrado DIGESA/DIGEMID) para devolver 422 legible en vez de
    // dejar que la violación se propague como una excepción SQL cruda (HU-15/HU-21, ABET 2).
    private static string? ValidarRegistroSanitario(string? registroSanitario, string? entidadRegistro)
    {
        var tieneRegistro = !string.IsNullOrWhiteSpace(registroSanitario);
        var tieneEntidad = !string.IsNullOrWhiteSpace(entidadRegistro);

        if (tieneRegistro != tieneEntidad)
        {
            return "registroSanitario y entidadRegistro deben declararse juntos, o ambos quedar vacíos (ABET 2).";
        }

        if (tieneEntidad && entidadRegistro is not ("DIGESA" or "DIGEMID"))
        {
            return "entidadRegistro solo admite 'DIGESA' o 'DIGEMID'.";
        }

        return null;
    }

    private static ProductoDetalleResponseDto MapearDetalle(
        Producto producto,
        IEnumerable<ProductoIngrediente> ingredientes,
        IEnumerable<ProductoContraindicacion> contraindicaciones,
        IEnumerable<Alergeno> alergenos) => new()
        {
            IdProducto = producto.IdProducto,
            Nombre = producto.Nombre,
            Marca = producto.Marca,
            Categoria = producto.Categoria,
            Objetivo = producto.Objetivo,
            Precio = producto.Precio,
            Descripcion = producto.Descripcion,
            DosisRecomendada = producto.DosisRecomendada,
            UrlImagen = producto.UrlImagen,
            RegistroSanitario = producto.RegistroSanitario,
            EntidadRegistro = producto.EntidadRegistro,
            Stock = producto.Stock,
            Ingredientes = ingredientes.Select(i => i.Ingrediente).ToList(),
            Contraindicaciones = contraindicaciones.Select(c => c.Descripcion).ToList(),
            Alergenos = alergenos.Select(a => a.Nombre).ToList(),
        };
}
