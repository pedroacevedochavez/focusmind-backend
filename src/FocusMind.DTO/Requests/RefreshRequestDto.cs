using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

public sealed class RefreshRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
