using System.Data;
using System.Data.Common;
using Shitty.Data.Ado.Schema;

namespace Shitty.Data.Ado
{
    public interface IConnectionProvider
    {
        void SetConnectionString(string connectionString);
        IDbConnection CreateConnection();
        ISchemaProvider GetSchemaProvider();
        string ConnectionString { get; }
        bool SupportsCompoundStatements { get; }
        string GetIdentityFunction();
        bool SupportsStoredProcedures { get; }
        IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName);
    }
}