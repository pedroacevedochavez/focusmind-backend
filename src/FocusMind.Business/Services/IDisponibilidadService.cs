using FocusMind.DTO.Requests;
using FocusMind.DTO.Responses;

namespace FocusMind.Business.Services;

public interface IDisponibilidadService
{
    Task<ValidarDisponibilidadResponseDto> ValidarAsync(IEnumerable<ItemCarritoDto> items);
}
