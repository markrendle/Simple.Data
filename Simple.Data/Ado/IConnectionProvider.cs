using System.Data;

namespace Simple.Data.Ado
{
    public interface IConnectionProvider
    {
        IDbConnection CreateConnection();
    }
}