using System.Data;

namespace FocusMind.DBContext.Common;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
