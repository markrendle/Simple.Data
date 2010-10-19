using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

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

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetSchema(string collectionName)
        {
            using (var cn = CreateConnection())
            {
                cn.Open();
                if (collectionName.Equals("primarykeys", StringComparison.InvariantCultureIgnoreCase))
                {
                    
                }
                return cn.GetSchema(collectionName);
            }
        }

        public DataTable GetSchema(string collectionName, params string[] restrictionValues)
        {
            using (var cn = CreateConnection())
            {
                cn.Open();
                return cn.GetSchema(collectionName, restrictionValues);
            }
        }

        private DataTable GetPrimaryKeys()
        {
            return SelectToDataTable(Properties.Resources.PrimaryKeySql);
        }

        private DataTable GetForeignKeys()
        {
            return SelectToDataTable(Properties.Resources.ForeignKeysSql);
        }

        private DataTable SelectToDataTable(string sql)
        {
            var dataTable = new DataTable();
            using (var cn = CreateConnection() as SqlConnection)
            {
                using (var adapter = new SqlDataAdapter(sql, cn))
                {
                    adapter.Fill(dataTable);
                }
            }

            return dataTable;
            
        }
    }
}
