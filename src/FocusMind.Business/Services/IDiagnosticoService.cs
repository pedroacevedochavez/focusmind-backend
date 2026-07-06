using FocusMind.DTO.Requests;
using FocusMind.DTO.Responses;

namespace FocusMind.Business.Services;

public interface IDiagnosticoService
{
    // idUsuarioAutenticado == null => calcula en memoria, sin persistencia (regla de negocio HU-16).
    Task<DiagnosticoResponseDto> ProcesarAsync(DiagnosticoCrearRequestDto dto, int? idUsuarioAutenticado);

    Task<IEnumerable<DiagnosticoListItemResponseDto>> ListarXUsuarioAsync(int idUsuario);

    Task<PerfilCognitivoResponseDto?> ObtenerPerfilCognitivoAsync(int idUsuario);
}
