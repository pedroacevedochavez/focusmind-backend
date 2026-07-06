namespace FocusMind.DTO.Responses;

public sealed class DisponibilidadItemResponseDto
{
    public int IdProducto { get; set; }
    public int CantidadSolicitada { get; set; }
    public bool Disponible { get; set; }

    // Null cuando Disponible = true. Motivos posibles: producto inexistente, inactivo, o
    // stock insuficiente (incluye la cantidad disponible real para que el Frontend pueda
    // ofrecer "ajustar cantidad" en vez de solo rechazar).
    public string? Motivo { get; set; }
}
