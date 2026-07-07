using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

public sealed class LoginRequestDto
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    // Sin MinLength a propósito: revelar la política de longitud mínima en el endpoint de
    // login (a diferencia de registro) ayudaría a un atacante a descartar candidatos más
    // rápido. El tope superior sí se mantiene (mismo criterio que RegistroRequestDto.Password).
    [Required, StringLength(100)]
    public string Password { get; set; } = string.Empty;
}
