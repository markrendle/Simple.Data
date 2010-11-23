using System.Data.Common;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    public interface IConnectionProvider
    {
        void SetConnectionString(string connectionString);
        DbConnection CreateConnection();
        ISchemaProvider GetSchemaProvider();
        string ConnectionString { get; }
    }
}