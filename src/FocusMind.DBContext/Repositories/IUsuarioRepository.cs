using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorEmailAsync(string email);

    Task<Usuario?> ObtenerPorIdAsync(int idUsuario);

    Task<int> InsertarAsync(string nombre, string email, string passwordHash, int? usuarioCrea);
}
