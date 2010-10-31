using System;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Data;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.SqlCe35
{
    [Export("sdf", typeof(IConnectionProvider))]
    public class SqlCe35ConnectionProvider : IConnectionProvider
    {
        private string _connectionString;

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new SqlCeConnection(_connectionString);
        }

        public ISchemaProvider GetSchemaProvider()
        {
            return new SqlCe35SchemaProvider(this);
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }
    }
}
