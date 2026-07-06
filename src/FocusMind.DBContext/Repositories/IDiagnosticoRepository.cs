using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public interface IDiagnosticoRepository
{
    Task<IEnumerable<Diagnostico>> ListarXUsuarioAsync(int idUsuario);

    Task<PerfilCognitivo?> ObtenerPerfilCognitivoXUsuarioAsync(int idUsuario);

    Task<int> InsertarAsync(
        Diagnostico diagnostico,
        IEnumerable<int> alergiaIds,
        IEnumerable<int> recomendacionIds,
        int? usuarioCrea);
}
