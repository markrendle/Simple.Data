using System;
using System.ComponentModel.Composition;
using System.Data;
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

        public IDbConnection CreateConnection()
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

        public string GetIdentityFunction()
        {
            return "@@IDENTITY";
        }

        public bool SupportsCompoundStatements
        {
            get { return false; }
        }

        public bool SupportsStoredProcedures
        {
            get { return false; }
        }

        public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            throw new NotSupportedException("SQL Server Compact Edition does not support stored procedures.");
        }
    }
}
