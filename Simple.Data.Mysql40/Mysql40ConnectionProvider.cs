using System;
using System.ComponentModel.Composition;
using System.Data.Common;
using MySql.Data.MySqlClient;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Mysql40
{
    [Export("sql", typeof(IConnectionProvider))]
    public class Mysql40ConnectionProvider : IConnectionProvider
    {
        private string _connectionString;
        
        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
        
        public ISchemaProvider GetSchemaProvider()
        {
            return new Mysql40SchemaProvider(this);
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public string GetIdentityFunction()
        {
            return "@@IDENTITY";
        }

        public bool SupportsStoredProcedures
        {
            get { return false; }
        }

        public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            throw new NotImplementedException();
        }

        public bool SupportsCompoundStatements
        {
            get { return false; }
        }
    }
}
