using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

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
            //Implicit foreign key support
            //MyIsam (the most used Mysql db engine) does not support foreign key constraint
            //Foreign key support is therefor implemented in an implicit way
            //based on naming conventions
            //If a column name exisits as a primarykey in one table, then a column with the same name can
            //be used as a foreign key in another table.
            var foreignKeys = new List<ForeignKey>();
            var tables = GetTables();
            var primaryKeys = tables.Select(t => new Tuple<Table, Key>(t, GetPrimaryKey(t))).ToList();

            foreach (var column in table.Columns)
            {
                foreignKeys.AddRange(
                    primaryKeys.Where(key => key.Item2[0].Contains(column.ActualName) && key.Item1.ActualName != table.ActualName).Select(
                        key =>
                        new ForeignKey(new ObjectName(null, table.ActualName),
                                       new List<string> {column.ActualName},new ObjectName(null, key.Item1.ActualName), new List<string> {key.Item2[0]}
                                       )));
            }
            return foreignKeys;
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
