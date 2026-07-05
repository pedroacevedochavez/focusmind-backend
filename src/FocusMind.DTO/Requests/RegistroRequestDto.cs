using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

public sealed class RegistroRequestDto
{
    [Required, MinLength(3)]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
