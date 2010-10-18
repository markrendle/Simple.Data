using System.Data;
using System.Data.Common;

namespace Simple.Data.Ado
{
    public interface IConnectionProvider
    {
        void SetConnectionString(string connectionString);
        DbConnection CreateConnection();
        DataTable GetSchema(string collectionName);
        DataTable GetSchema(string collectionName, params string[] restrictionValues);
    }
}