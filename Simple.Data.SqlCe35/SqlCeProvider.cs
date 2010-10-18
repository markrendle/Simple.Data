using System;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Data;
using Simple.Data.Ado;

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

        public DataTable GetSchema(string collectionName)
        {
            if (collectionName.Equals("tables", StringComparison.InvariantCultureIgnoreCase))
            {
                var table = new DataTable(collectionName);
                using (
                    var dataAdapter =
                        new SqlCeDataAdapter(
                            "select table_name, table_schema, table_type from information_schema.tables",
                            _connectionString))
                {
                    dataAdapter.Fill(table);
                }
                return table;
            }

            return null;
        }

        public DataTable GetSchema(string collectionName, params string[] restrictionValues)
        {
            if (collectionName.Equals("columns", StringComparison.InvariantCultureIgnoreCase))
            {
                var table = new DataTable(collectionName);
                using (var cmd = new SqlCeCommand("select table_name, table_schema, column_name from information_schema.columns",
                            new SqlCeConnection(_connectionString)))
                {
                    if (restrictionValues.Length > 2 && restrictionValues[2] != null)
                    {
                        cmd.CommandText += " where table_name = @name";
                        cmd.Parameters.AddWithValue("@name", restrictionValues[2]);
                    }
                    using (var dataAdapter = new SqlCeDataAdapter(cmd))
                    {
                        dataAdapter.Fill(table);
                    }
                }
                return table;
            }

            return null;
        }
    }
}
