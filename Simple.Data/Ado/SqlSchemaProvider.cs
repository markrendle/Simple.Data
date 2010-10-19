using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    class SqlSchemaProvider : ISchemaProvider
    {
        private readonly IConnectionProvider _connectionProvider;

        public SqlSchemaProvider(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public DataTable GetSchema(string collectionName)
        {
            using (var cn = _connectionProvider.CreateConnection())
            {
                cn.Open();
                if (collectionName.Equals("primarykeys", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GetPrimaryKeys();
                }
                if (collectionName.Equals("foreignkeys", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GetForeignKeys();
                }
                return cn.GetSchema(collectionName);
            }
        }

        public DataTable GetSchema(string collectionName, params string[] restrictionValues)
        {
            if (collectionName.Equals("primarykeys", StringComparison.InvariantCultureIgnoreCase))
            {
                return GetPrimaryKeys(restrictionValues[0]);
            }
            if (collectionName.Equals("foreignkeys", StringComparison.InvariantCultureIgnoreCase))
            {
                return GetForeignKeys(restrictionValues[0]);
            }
            using (var cn = _connectionProvider.CreateConnection())
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

        private DataTable GetPrimaryKeys(string tableName)
        {
            return GetPrimaryKeys().AsEnumerable()
                .Where(
                    row => row["TABLE_NAME"].ToString().Equals(tableName, StringComparison.InvariantCultureIgnoreCase))
                .CopyToDataTable();
        }

        private DataTable GetForeignKeys(string tableName)
        {
            return GetForeignKeys().AsEnumerable()
                .Where(
                    row => row["TABLE_NAME"].ToString().Equals(tableName, StringComparison.InvariantCultureIgnoreCase))
                .CopyToDataTable();
        }

        private DataTable SelectToDataTable(string sql)
        {
            var dataTable = new DataTable();
            using (var cn = _connectionProvider.CreateConnection() as SqlConnection)
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
