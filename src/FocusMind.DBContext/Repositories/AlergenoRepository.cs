using System.Data;
using Dapper;
using FocusMind.DBContext.Common;
using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public sealed class AlergenoRepository(IDbConnectionFactory connectionFactory) : IAlergenoRepository
{
    public async Task<IEnumerable<Alergeno>> ListarAsync()
    {
        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Alergeno>(
            "usp_ListarAlergeno",
            commandType: CommandType.StoredProcedure);
    }
}
