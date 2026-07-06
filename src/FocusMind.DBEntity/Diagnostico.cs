namespace FocusMind.DBEntity;

public class Diagnostico
{
    public int IdDiagnostico { get; set; }
    public int IdUsuario { get; set; }
    public DateTime Fecha { get; set; }
    public int NivelEstres { get; set; }
    public int CalidadSueno { get; set; }
    public int IdObjetivo { get; set; }
    public int HorasConcentracion { get; set; }
    public string? CondicionMedica { get; set; }
}
