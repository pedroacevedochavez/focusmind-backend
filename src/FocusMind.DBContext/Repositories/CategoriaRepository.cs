using System.Data;
using Dapper;
using FocusMind.DBContext.Common;
using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public sealed class CategoriaRepository(IDbConnectionFactory connectionFactory) : ICategoriaRepository
{
    public async Task<IEnumerable<Categoria>> ListarAsync()
    {
        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Categoria>(
            "usp_ListarCategoria",
            commandType: CommandType.StoredProcedure);
    }
}
