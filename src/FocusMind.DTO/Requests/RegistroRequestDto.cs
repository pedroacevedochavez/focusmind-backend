using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

public sealed class RegistroRequestDto
{
    // Límites alineados a TM_USUARIO.NOMBRE VARCHAR(100)/EMAIL VARCHAR(254) — un valor que
    // pase la validación pero exceda la columna solo se descubriría como un 500 de SQL
    // (truncamiento) en vez de un 400 claro; HU-20 cierra ese hueco.
    [Required, MinLength(3), StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    // 100 es un tope defensivo propio (no hay columna: Password nunca se persiste en texto
    // plano, solo su hash BCrypt en TM_USUARIO.PASSWORD VARCHAR(256)) — evita que un input
    // absurdamente largo se procese contra BCrypt (costoso por diseño) sin ganar nada en
    // seguridad real.
    [Required, MinLength(8), StringLength(100)]
    public string Password { get; set; } = string.Empty;
}
