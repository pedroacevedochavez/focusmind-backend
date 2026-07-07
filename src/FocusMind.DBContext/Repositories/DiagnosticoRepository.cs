using System.Data;
using Dapper;
using FocusMind.DBContext.Common;
using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public sealed class DiagnosticoRepository(IDbConnectionFactory connectionFactory) : IDiagnosticoRepository
{
    public async Task<IEnumerable<Diagnostico>> ListarXUsuarioAsync(int idUsuario)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDUSUARIO", idUsuario);

        return await connection.QueryAsync<Diagnostico>(
            "usp_Listar_Diagnostico_X_Usuario",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PerfilCognitivo?> ObtenerPerfilCognitivoXUsuarioAsync(int idUsuario)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDUSUARIO", idUsuario);

        return await connection.QuerySingleOrDefaultAsync<PerfilCognitivo>(
            "usp_ObtenerPerfilCognitivo_X_Usuario",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Alergeno>> ListarAlergenosAsync(int idDiagnostico)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDDIAGNOSTICO", idDiagnostico);

        return await connection.QueryAsync<Alergeno>(
            "usp_Listar_DiagnosticoAlergeno_X_Diagnostico",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ProductoRecomendado>> ListarRecomendacionesAsync(int idDiagnostico)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDDIAGNOSTICO", idDiagnostico);

        return await connection.QueryAsync<ProductoRecomendado>(
            "usp_Listar_DiagnosticoRecomendacion_X_Diagnostico",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    // Mismo patrón transaccional que ProductoRepository.InsertarAsync (HU-15): encabezado +
    // N alergias + N recomendaciones en una sola transacción ADO.NET — si algo falla, rollback
    // total (ej. un IDALERGENO o IDPRODUCTO ya no vigente).
    public async Task<int> InsertarAsync(
        Diagnostico diagnostico,
        IEnumerable<int> alergiaIds,
        IEnumerable<int> recomendacionIds,
        int? usuarioCrea)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var parametrosDiagnostico = new DynamicParameters();
            parametrosDiagnostico.Add("@IDUSUARIO", diagnostico.IdUsuario);
            parametrosDiagnostico.Add("@FECHA", diagnostico.Fecha);
            parametrosDiagnostico.Add("@NIVELESTRES", diagnostico.NivelEstres);
            parametrosDiagnostico.Add("@CALIDADSUENO", diagnostico.CalidadSueno);
            parametrosDiagnostico.Add("@IDOBJETIVO", diagnostico.IdObjetivo);
            parametrosDiagnostico.Add("@HORASCONCENTRACION", diagnostico.HorasConcentracion);
            parametrosDiagnostico.Add("@CONDICIONMEDICA", diagnostico.CondicionMedica);
            parametrosDiagnostico.Add("@USUARIOCREA", usuarioCrea);
            parametrosDiagnostico.Add("@IDDIAGNOSTICO", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "usp_InsertarDiagnostico",
                parametrosDiagnostico,
                transaction: transaction,
                commandType: CommandType.StoredProcedure);

            var idDiagnostico = parametrosDiagnostico.Get<int>("@IDDIAGNOSTICO");

            foreach (var idAlergeno in alergiaIds)
            {
                var parametrosAlergia = new DynamicParameters();
                parametrosAlergia.Add("@IDDIAGNOSTICO", idDiagnostico);
                parametrosAlergia.Add("@IDALERGENO", idAlergeno);
                parametrosAlergia.Add("@USUARIOCREA", usuarioCrea);
                parametrosAlergia.Add("@IDDIAGNOSTICOALERGENO", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "usp_InsertarDiagnosticoAlergeno",
                    parametrosAlergia,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }

            foreach (var idProducto in recomendacionIds)
            {
                var parametrosRecomendacion = new DynamicParameters();
                parametrosRecomendacion.Add("@IDDIAGNOSTICO", idDiagnostico);
                parametrosRecomendacion.Add("@IDPRODUCTO", idProducto);
                parametrosRecomendacion.Add("@USUARIOCREA", usuarioCrea);
                parametrosRecomendacion.Add("@IDDIAGNOSTICORECOMENDACION", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "usp_InsertarDiagnosticoRecomendacion",
                    parametrosRecomendacion,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }

            transaction.Commit();
            return idDiagnostico;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
