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

    Task<bool> EliminarAsync(int idProducto, int? usuarioModifica);
}
