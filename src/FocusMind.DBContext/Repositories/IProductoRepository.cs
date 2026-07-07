using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public interface IProductoRepository
{
    Task<IEnumerable<Producto>> ListarAsync();

    Task<Producto?> ObtenerAsync(int idProducto);

    Task<IEnumerable<ProductoIngrediente>> ListarIngredientesAsync(int idProducto);

    Task<IEnumerable<ProductoContraindicacion>> ListarContraindicacionesAsync(int idProducto);

    Task<IEnumerable<Alergeno>> ListarAlergenosAsync(int idProducto);

    Task<int> InsertarAsync(
        Producto producto,
        IEnumerable<string> ingredientes,
        IEnumerable<string> contraindicaciones,
        IEnumerable<int> alergenoIds,
        int? usuarioCrea);

    Task<bool> ActualizarAsync(Producto producto, int? usuarioModifica);

    // HU-21 — Transparencia sanitaria persistente: sobrecarga usada por el flujo de edición
    // completa (PUT /api/productos/:id). A diferencia de ActualizarAsync(Producto, int?) —que
    // solo toca TM_PRODUCTO y la sigue usando EliminarAsync para la baja lógica del producto sin
    // afectar sus hijos—, esta versión resincroniza ingredientes/contraindicaciones/alérgenos en
    // la MISMA transacción ACID que el UPDATE del producto.
    Task<bool> ActualizarAsync(
        Producto producto,
        IEnumerable<string> ingredientes,
        IEnumerable<string> contraindicaciones,
        IEnumerable<int> alergenoIds,
        int? usuarioModifica);

    Task<bool> EliminarAsync(int idProducto, int? usuarioModifica);
}
