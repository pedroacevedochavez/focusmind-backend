namespace FocusMind.DBEntity;

public class Categoria
{
    public int IdCategoria { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
