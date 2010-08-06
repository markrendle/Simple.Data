using System.Data;

namespace Simple.Data.SqlCe
{
    public interface IConnectionProvider
    {
        IDbConnection CreateConnection();
    }
}