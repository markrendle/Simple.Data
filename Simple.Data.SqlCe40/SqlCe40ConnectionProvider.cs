using System;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Data.SqlServerCe;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.SqlCe40
{
    [Export("sdf", typeof(IConnectionProvider))]
    public class SqlCe40ConnectionProvider : IConnectionProvider
    {
        private string _connectionString;
        private bool _checked;

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            if (!_checked) CheckVersion();
            return new SqlCeConnection(_connectionString);
        }

        private void CheckVersion()
        {
            try
            {
                using (var cn = new SqlCeConnection(_connectionString))
                {
                    cn.Open();
                }
            }
            catch (SqlCeInvalidDatabaseFormatException)
            {
                new SqlCeEngine(_connectionString).Upgrade();
            }
            _checked = true;
        }

        public ISchemaProvider GetSchemaProvider()
        {
            return new SqlCe40SchemaProvider(this);
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }
    }
}
