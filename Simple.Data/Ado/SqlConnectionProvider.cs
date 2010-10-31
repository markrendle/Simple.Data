using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    class SqlConnectionProvider : IConnectionProvider
    {
        private string _connectionString;

        public SqlConnectionProvider()
        {
            
        }

        public SqlConnectionProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public ISchemaProvider GetSchemaProvider()
        {
            return new SqlSchemaProvider(this);
        }

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }
    }
}
