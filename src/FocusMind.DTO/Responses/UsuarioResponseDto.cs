namespace FocusMind.DTO.Responses;

public sealed class UsuarioResponseDto
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
