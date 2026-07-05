using System.Data;
using Dapper;
using FocusMind.DBContext.Common;
using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public sealed class ProductoRepository(IDbConnectionFactory connectionFactory) : IProductoRepository
{
    public async Task<IEnumerable<Producto>> ListarAsync()
    {
        using var connection = connectionFactory.CreateConnection();

        return await connection.QueryAsync<Producto>(
            "usp_ListarProducto",
            commandType: CommandType.StoredProcedure);
    }
}
