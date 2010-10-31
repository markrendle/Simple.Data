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
    public class SqlCe35Provider : IConnectionProvider
    {
        private string _connectionString;

        public SqlCe35Provider()
        {
            
        }

        public SqlCe35Provider(string filename)
        {
            _connectionString = string.Format("data source={0}", filename);
        }

        public SqlCe35Provider(string filename, string password)
        {
            _connectionString = string.Format("data source={0};password={1}", filename, password);
        }

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
            return new SqlCeSchemaProvider(this);
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }
    }
}
