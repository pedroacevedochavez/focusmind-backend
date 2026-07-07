using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public interface IDiagnosticoRepository
{
    Task<IEnumerable<Diagnostico>> ListarXUsuarioAsync(int idUsuario);

    Task<PerfilCognitivo?> ObtenerPerfilCognitivoXUsuarioAsync(int idUsuario);

    // HU-19 (cierre del gap de "Mis Recomendaciones"): ambos SPs ya existían desde el diseño
    // original del esquema (usp_Listar_DiagnosticoAlergeno_X_Diagnostico y
    // usp_Listar_DiagnosticoRecomendacion_X_Diagnostico) — nunca se habían expuesto vía
    // repositorio porque HU-16 no los necesitaba para sus 3 endpoints originales.
    Task<IEnumerable<Alergeno>> ListarAlergenosAsync(int idDiagnostico);

    Task<IEnumerable<ProductoRecomendado>> ListarRecomendacionesAsync(int idDiagnostico);

    Task<int> InsertarAsync(
        Diagnostico diagnostico,
        IEnumerable<int> alergiaIds,
        IEnumerable<int> recomendacionIds,
        int? usuarioCrea);
}
