namespace FocusMind.DTO.Responses;

public sealed class AuthResponseDto
{
    public UsuarioResponseDto Usuario { get; set; } = null!;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
