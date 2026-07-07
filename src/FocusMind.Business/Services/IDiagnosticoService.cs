using FocusMind.DTO.Requests;
using FocusMind.DTO.Responses;

namespace FocusMind.Business.Services;

public interface IDiagnosticoService
{
    // idUsuarioAutenticado == null => calcula en memoria, sin persistencia (regla de negocio HU-16).
    Task<DiagnosticoResponseDto> ProcesarAsync(DiagnosticoCrearRequestDto dto, int? idUsuarioAutenticado);

    Task<IEnumerable<DiagnosticoListItemResponseDto>> ListarXUsuarioAsync(int idUsuario);

    // HU-19: devuelve el mismo shape enriquecido que ProcesarAsync (alergias + recomendaciones
    // incluidas), no solo los 3 campos de perfil — cierra el gap del panel "Mis Recomendaciones"
    // del Dashboard. null si el usuario no tiene ningún diagnóstico registrado.
    Task<DiagnosticoResponseDto?> ObtenerUltimoAsync(int idUsuario);
}
