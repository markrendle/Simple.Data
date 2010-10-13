using System.Data;

namespace Simple.Data.Ado
{
    public interface IConnectionProvider
    {
        void SetConnectionString(string connectionString);
        IDbConnection CreateConnection();
    }
}