using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

public sealed class RefreshRequestDto
{
    // 2000 caracteres es generoso frente a lo que JwtTokenGenerator realmente emite; solo
    // busca rechazar temprano un body manipulado/gigante antes de tocar la BD.
    [Required, StringLength(2000)]
    public string RefreshToken { get; set; } = string.Empty;
}
