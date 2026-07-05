using System.Data;
using Dapper;
using FocusMind.DBContext.Common;
using FocusMind.DBEntity;

namespace FocusMind.DBContext.Repositories;

public sealed class UsuarioRepository(IDbConnectionFactory connectionFactory) : IUsuarioRepository
{
    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@EMAIL", email);

        return await connection.QuerySingleOrDefaultAsync<Usuario>(
            "usp_ObtenerUsuario_X_Email",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int idUsuario)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@IDUSUARIO", idUsuario);

        return await connection.QuerySingleOrDefaultAsync<Usuario>(
            "usp_ObtenerUsuario",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> InsertarAsync(string nombre, string email, string passwordHash, int? usuarioCrea)
    {
        using var connection = connectionFactory.CreateConnection();

        var parametros = new DynamicParameters();
        parametros.Add("@NOMBRE", nombre);
        parametros.Add("@EMAIL", email);
        parametros.Add("@PASSWORD", passwordHash);
        parametros.Add("@USUARIOCREA", usuarioCrea);
        parametros.Add("@IDUSUARIO", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "usp_InsertarUsuario",
            parametros,
            commandType: CommandType.StoredProcedure);

        return parametros.Get<int>("@IDUSUARIO");
    }
}
