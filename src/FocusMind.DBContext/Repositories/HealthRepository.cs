using Dapper;
using FocusMind.DBContext.Common;

namespace FocusMind.DBContext.Repositories;

public sealed class HealthRepository(IDbConnectionFactory connectionFactory) : IHealthRepository
{
    public async Task<bool> VerificarConexionAsync()
    {
        using var connection = connectionFactory.CreateConnection();

        var resultado = await connection.QuerySingleAsync<int>("SELECT 1");

        return resultado == 1;
    }
}
