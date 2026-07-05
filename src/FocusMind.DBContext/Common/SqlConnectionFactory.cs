using System.Data;
using Microsoft.Data.SqlClient;

namespace FocusMind.DBContext.Common;

public sealed class SqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection(connectionString);
}
