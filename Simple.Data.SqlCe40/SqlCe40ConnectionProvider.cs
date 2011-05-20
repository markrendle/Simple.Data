using System;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Linq;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.SqlCe40
{
    [Export(typeof(IConnectionProvider))]
    [Export("sdf", typeof(IConnectionProvider))]
    [Export("System.Data.SqlServerCe", typeof(IConnectionProvider))]
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

        public bool TryGetNewRowSelect(Table table, out string selectSql)
        {
            var identityColumn = table.Columns.FirstOrDefault(col => col.IsIdentity);

            if (identityColumn == null)
            {
                selectSql = null;
                return false;
            }

            selectSql = "select * from " + table.QualifiedName + " where " + identityColumn.QuotedName +
                        " = @@IDENTITY";
            return true;
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
