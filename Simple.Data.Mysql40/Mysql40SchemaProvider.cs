using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.Mysql40.Properties;

namespace Simple.Data.Mysql40
{
    class Mysql40SchemaProvider : ISchemaProvider
    {
        private readonly IConnectionProvider _connectionProvider;

        public Mysql40SchemaProvider(IConnectionProvider connectionProvider)
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            _connectionProvider = connectionProvider;
        }

        public IConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        public IEnumerable<Table> GetTables()
        {
            using (var cn = ConnectionProvider.CreateConnection())
            {
                var command = cn.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "SHOW TABLES;";
                cn.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new Table(reader[0].ToString(),null, TableType.Table);
                    }
                }
                
            }
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            return Enumerable.Select(GetColumnsDataTable(table).AsEnumerable(), row => new Column(row["Field"].ToString(), table, row["Extra"].ToString().ToUpper().Contains("AUTO_INCREMENT")));
        }

        public IEnumerable<Procedure> GetStoredProcedures()
        {
            return Enumerable.Empty<Procedure>();
        }

       public IEnumerable<Parameter> GetParameters(Procedure storedProcedure)
        {
            return Enumerable.Empty<Parameter>();
        }

        public Key GetPrimaryKey(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");
            return
                new Key (GetColumnsDataTable(table).AsEnumerable().Where(c => c["Key"].ToString().ToUpper() == "PRI").Select(
                    c => c["Field"].ToString()));

        }

        public IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");
            var groups = GetForeignKeys(table.ActualName)
                .Where(row => AreEqual(row["TABLE_SCHEMA"], table.Schema)
                    && row["TABLE_NAME"].ToString() == table.ActualName)
                .GroupBy(row => row["CONSTRAINT_NAME"].ToString())
                .ToList();

            foreach (var group in groups)
            {
                yield return new ForeignKey(new ObjectName(group.First()["TABLE_SCHEMA"], group.First()["TABLE_NAME"]),
                    group.Select(row => row["COLUMN_NAME"].ToString()),
                    new ObjectName(group.First()["UNIQUE_TABLE_SCHEMA"], group.First()["UNIQUE_TABLE_NAME"]),
                    group.Select(row => row["UNIQUE_COLUMN_NAME"].ToString()));
            }
        }

        private static bool AreEqual(object string1, object string2)
        {
            if (string1 == DBNull.Value) string1 = null;
            if (string2 == DBNull.Value) string2 = null;
            if (string1 == null && string2 == null) return true;
            if (string1 == null || string2 == null) return false;
            return string1.ToString().Trim().Equals(string2.ToString().Trim());
        }

        public string QuoteObjectName(string unquotedName)
        {
            //no quoting of table / column name in MySql 4.0
            if (unquotedName == null) throw new ArgumentNullException("unquotedName");
            return unquotedName;
        }

        public string NameParameter(string baseName)
        {
            if (baseName == null) throw new ArgumentNullException("baseName");
            if (baseName.Length == 0) throw new ArgumentException("Base name must be provided");
            return (baseName.StartsWith("?")) ? baseName : "?" + baseName;
        }

        public Type DataTypeToClrType(string dataType)
        {
            return SqlTypeResolver.GetClrType(dataType);
        }

        private DataTable GetColumnsDataTable(Table table)
        {
            return SelectToDataTable(string.Format("SHOW COLUMNS FROM {0}",table.ActualName));
        }

        private DataTable GetPrimaryKeys()
        {
            return SelectToDataTable(Resources.PrimaryKeysSql);
        }

        private DataTable GetForeignKeys()
        {
            return SelectToDataTable(Resources.ForeignKeysSql);
        }

        private EnumerableRowCollection<DataRow> GetForeignKeys(string tableName)
        {
            return GetForeignKeys().AsEnumerable()
                .Where(
                    row => row["TABLE_NAME"].ToString().Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
        }

        private DataTable SelectToDataTable(string sql)
        {
            var dataTable = new DataTable();
            using (var cn = ConnectionProvider.CreateConnection() as MySqlConnection)
            {
                using (var adapter = new MySqlDataAdapter(sql, cn))
                {
                    adapter.Fill(dataTable);
                }

            }

            return dataTable;
        }
    }
}
