using System.ComponentModel.DataAnnotations;

namespace FocusMind.DTO.Requests;

// HU-20/HU-21: [StringLength]/[MaxLength] no validan el largo de cada elemento de un
// List<string> — solo el conteo de la lista. Se centraliza aquí (en vez de duplicarla en
// ProductoCrearRequestDto y ProductoActualizarRequestDto) porque ambos DTOs comparten el mismo
// invariante derivado del esquema real: TD_PRODUCTO_INGREDIENTE.INGREDIENTE VARCHAR(150) y
// TD_PRODUCTO_CONTRAINDICACION.DESCRIPCION VARCHAR(300) (01_Schema_Tablas.sql). Si esas
// columnas cambian de tamaño algún día, solo hay un lugar que actualizar.
internal static class ProductoListasSanitariasValidacion
{
    private const int MaxLargoIngrediente = 150;
    private const int MaxLargoContraindicacion = 300;

    public static IEnumerable<ValidationResult> Validar(
        IReadOnlyList<string> ingredientes,
        IReadOnlyList<string> contraindicaciones,
        string nombreCampoIngredientes,
        string nombreCampoContraindicaciones)
    {
        for (var i = 0; i < ingredientes.Count; i++)
        {
            if (ingredientes[i].Length > MaxLargoIngrediente)
            {
                yield return new ValidationResult(
                    $"{nombreCampoIngredientes}[{i}] excede el máximo de {MaxLargoIngrediente} caracteres.",
                    [nombreCampoIngredientes]);
            }
        }

        for (var i = 0; i < contraindicaciones.Count; i++)
        {
            if (contraindicaciones[i].Length > MaxLargoContraindicacion)
            {
                yield return new ValidationResult(
                    $"{nombreCampoContraindicaciones}[{i}] excede el máximo de {MaxLargoContraindicacion} caracteres.",
                    [nombreCampoContraindicaciones]);
            }
        }
    }
}
