using FocusMind.DBContext.Repositories;
using FocusMind.DBEntity;
using FocusMind.DTO.Requests;
using FocusMind.DTO.Responses;

namespace FocusMind.Business.Services;

public sealed class DiagnosticoService(
    IDiagnosticoRepository diagnosticoRepository,
    IObjetivoRepository objetivoRepository,
    IAlergenoRepository alergenoRepository,
    IMotorRecomendacionService motorRecomendacion) : IDiagnosticoService
{
    private const int LimiteRecomendaciones = 3;

    // Regla de negocio HU-16 (diagnóstico público y dinámico):
    //   - idUsuarioAutenticado == null -> se calcula 100% en memoria, CERO escrituras en RDS
    //     (ni encabezado, ni alergias, ni recomendaciones).
    //   - idUsuarioAutenticado != null -> se persiste todo en una única transacción (mismo
    //     patrón que ProductoRepository.InsertarAsync en HU-15).
    // El motor de recomendación (MotorRecomendacionService) corre IGUAL en ambos casos —
    // la única diferencia es si el resultado se guarda o no.
    public async Task<DiagnosticoResponseDto> ProcesarAsync(DiagnosticoCrearRequestDto dto, int? idUsuarioAutenticado)
    {
        var objetivo = await objetivoRepository.ObtenerAsync(dto.IdObjetivo);

        var catalogoAlergenos = await alergenoRepository.ListarAsync();
        var nombresAlergias = catalogoAlergenos
            .Where(a => dto.AlergiaIds.Contains(a.IdAlergeno))
            .Select(a => a.Nombre)
            .ToList();

        var recomendaciones = (await motorRecomendacion.RecomendarAsync(dto.IdObjetivo, dto.AlergiaIds, LimiteRecomendaciones)).ToList();
        var fecha = DateTime.UtcNow;

        if (idUsuarioAutenticado is null)
        {
            return Mapear(idDiagnostico: null, fecha, objetivo, dto, nombresAlergias, recomendaciones, persistido: false);
        }

        var diagnostico = new Diagnostico
        {
            IdUsuario = idUsuarioAutenticado.Value,
            Fecha = fecha,
            NivelEstres = dto.NivelEstres,
            CalidadSueno = dto.CalidadSueno,
            IdObjetivo = dto.IdObjetivo,
            HorasConcentracion = dto.HorasConcentracion,
            CondicionMedica = dto.CondicionMedica,
        };

        var idDiagnostico = await diagnosticoRepository.InsertarAsync(
            diagnostico,
            dto.AlergiaIds,
            recomendaciones.Select(p => p.IdProducto),
            idUsuarioAutenticado);

        return Mapear(idDiagnostico, fecha, objetivo, dto, nombresAlergias, recomendaciones, persistido: true);
    }

    public async Task<IEnumerable<DiagnosticoListItemResponseDto>> ListarXUsuarioAsync(int idUsuario)
    {
        var diagnosticos = await diagnosticoRepository.ListarXUsuarioAsync(idUsuario);

        var resultado = new List<DiagnosticoListItemResponseDto>();

        // N+1 deliberado (máx. 10 filas, mismo criterio ya documentado en HU-15).
        foreach (var diagnostico in diagnosticos)
        {
            var objetivo = await objetivoRepository.ObtenerAsync(diagnostico.IdObjetivo);

            resultado.Add(new DiagnosticoListItemResponseDto
            {
                IdDiagnostico = diagnostico.IdDiagnostico,
                Fecha = diagnostico.Fecha,
                NivelEstres = diagnostico.NivelEstres,
                CalidadSueno = diagnostico.CalidadSueno,
                Objetivo = objetivo?.Nombre ?? string.Empty,
                HorasConcentracion = diagnostico.HorasConcentracion,
                CondicionMedica = diagnostico.CondicionMedica,
            });
        }

        return resultado;
    }

    public async Task<PerfilCognitivoResponseDto?> ObtenerPerfilCognitivoAsync(int idUsuario)
    {
        var perfil = await diagnosticoRepository.ObtenerPerfilCognitivoXUsuarioAsync(idUsuario);
        if (perfil is null)
        {
            return null;
        }

        return new PerfilCognitivoResponseDto
        {
            NivelEstres = perfil.NivelEstres,
            CalidadSueno = perfil.CalidadSueno,
            ObjetivoPrincipal = perfil.ObjetivoPrincipal,
        };
    }

    private static DiagnosticoResponseDto Mapear(
        int? idDiagnostico,
        DateTime fecha,
        Objetivo? objetivo,
        DiagnosticoCrearRequestDto dto,
        List<string> alergias,
        List<Producto> recomendaciones,
        bool persistido) => new()
        {
            IdDiagnostico = idDiagnostico,
            Fecha = fecha,
            NivelEstres = dto.NivelEstres,
            CalidadSueno = dto.CalidadSueno,
            Objetivo = objetivo?.Nombre ?? string.Empty,
            HorasConcentracion = dto.HorasConcentracion,
            CondicionMedica = dto.CondicionMedica,
            Alergias = alergias,
            Recomendaciones = recomendaciones.Select(p => new ProductoRecomendadoResponseDto
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                Marca = p.Marca,
                Precio = p.Precio,
                UrlImagen = p.UrlImagen,
                Stock = p.Stock,
            }).ToList(),
            Persistido = persistido,
        };
}
