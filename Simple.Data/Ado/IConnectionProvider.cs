using System.Data.Common;

namespace Simple.Data.Ado
{
    public interface IConnectionProvider
    {
        void SetConnectionString(string connectionString);
        DbConnection CreateConnection();
        ISchemaProvider GetSchemaProvider();
    }
}