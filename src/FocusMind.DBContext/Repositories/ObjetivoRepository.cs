using System.Data;
using Dapper;
using FocusMind.DBContext.Common;
using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public sealed class ObjetivoRepository(IDbConnectionFactory connectionFactory) : IObjetivoRepository
{
    public async Task<Objetivo?> ObtenerAsync(int idObjetivo)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDOBJETIVO", idObjetivo);

        return await connection.QuerySingleOrDefaultAsync<Objetivo>(
            "usp_ObtenerObjetivo",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Objetivo>> ListarAsync()
    {
        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Objetivo>(
            "usp_ListarObjetivo",
            commandType: CommandType.StoredProcedure);
    }
}
