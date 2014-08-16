using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.SqlServer
{
    class SqlSchemaProvider : ISchemaProvider
    {
        private readonly IConnectionProvider _connectionProvider;
        private IEnumerable<Tuple<string, string>> _synonyms;

        public SqlSchemaProvider(IConnectionProvider connectionProvider)
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            _connectionProvider = connectionProvider;
            _synonyms = getSynonyms();
        }

        public IConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        public IEnumerable<Table> GetTables()
        {
            var tables = GetSchema("TABLES").Select(SchemaRowToTable);
            //Add Synonyms a table to the tables list
            List<Table> synTables = new List<Table>();
            foreach (var syn in _synonyms)
            {
                var tbl = tables.Where(t =>
                    syn.Item2.Equals(string.Format("{0}.{1}", t.Schema, t.ActualName), StringComparison.CurrentCultureIgnoreCase))
                    .FirstOrDefault();
                if (tbl != null)
                    synTables.Add(new Table(syn.Item1, tbl.Schema, tbl.Type, tbl.ActualName));
            }

            return tables.Union(synTables);
        }

        private static Table SchemaRowToTable(DataRow row)
        {
            return new Table(row["TABLE_NAME"].ToString(), row["TABLE_SCHEMA"].ToString(),
                        row["TABLE_TYPE"].ToString() == "BASE TABLE" ? TableType.Table : TableType.View);
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");
            Table resolvedTable = table;
            //if this table is a synonym, get columns from real table
            if (!string.IsNullOrEmpty(table.AliasedTable))
                resolvedTable = new Table(table.AliasedTable, table.Schema, table.Type);
            var cols = GetColumnsDataTable(resolvedTable);
            return cols.AsEnumerable().Select(row => SchemaRowToColumn(table, row));
        }

        private static Column SchemaRowToColumn(Table table, DataRow row)
        {
            SqlDbType sqlDbType = SqlDbType.Udt;

            if (!row.IsNull("type_name"))
                sqlDbType = DbTypeFromInformationSchemaTypeName((string)row["type_name"]);

            var size = (short)row["max_length"];
            switch (sqlDbType)
            {
                case SqlDbType.Image:
                case SqlDbType.NText:
                case SqlDbType.Text:
                    size = -1;
                    break;
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                    size = (short)(size / 2);
                    break;
            }

            return new SqlColumn(row["name"].ToString(), table, (bool)row["is_identity"], sqlDbType, size);
        }

        public IEnumerable<Procedure> GetStoredProcedures()
        {
            var sprocs = GetSchema("Procedures").Select(SchemaRowToStoredProcedure);
            //Add Synonyms a sproc to the sproc list
            List<Procedure> synProcs = new List<Procedure>();
            foreach (var syn in _synonyms)
            {
                var proc = sprocs.Where(p =>
                    syn.Item2.Equals(string.Format("{0}.{1}", p.Schema, p.SpecificName), StringComparison.CurrentCultureIgnoreCase))
                    .FirstOrDefault();
                if (proc != null)
                    synProcs.Add(new Procedure(syn.Item1, syn.Item1, proc.Schema, proc.SpecificName));
            }

            return sprocs.Union(synProcs);
        }

        private IEnumerable<DataRow> GetSchema(string collectionName, params string[] constraints)
        {
            using (var cn = ConnectionProvider.CreateConnection())
            {
                cn.Open();

                return cn.GetSchema(collectionName, constraints).AsEnumerable();
            }
        }

        private static Procedure SchemaRowToStoredProcedure(DataRow row)
        {
            return new Procedure(row["ROUTINE_NAME"].ToString(), row["SPECIFIC_NAME"].ToString(), row["ROUTINE_SCHEMA"].ToString());
        }

        public IEnumerable<Parameter> GetParameters(Procedure storedProcedure)
        {
            //support for aliased storec procedure names (synonyms)
            var sprocName = string.IsNullOrEmpty(storedProcedure.AliasedProcedure) ? storedProcedure.QualifiedName
                : (string.Format("[{0}].[{1}]", storedProcedure.Schema, storedProcedure.AliasedProcedure));
            // GetSchema does not return the return value of e.g. a stored proc correctly,
            // i.e. there isn't sufficient information to correctly set up a stored proc.
            using (var connection = (SqlConnection)ConnectionProvider.CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = sprocName;

                    connection.Open();
                    SqlCommandBuilder.DeriveParameters(command);

                    //Tim Cartwright: I added size and dbtype so inout/out params would function properly.
                    foreach (SqlParameter p in command.Parameters)
                        yield return new Parameter(p.ParameterName, SqlTypeResolver.GetClrType(p.DbType.ToString()), p.Direction, p.DbType, p.Size);
                }
            }
        }

        public Key GetPrimaryKey(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");
            var tableName = string.IsNullOrEmpty(table.AliasedTable) ? table.ActualName : table.AliasedTable;
            var primaryKeys = GetPrimaryKeys(tableName);
            if (primaryKeys == null)
            {
                return new Key(Enumerable.Empty<string>());
            }
            return new Key(primaryKeys.AsEnumerable()
                .Where(
                    row =>
                    row["TABLE_SCHEMA"].ToString() == table.Schema && row["TABLE_NAME"].ToString() == tableName)
                    .OrderBy(row => (int)row["ORDINAL_POSITION"])
                    .Select(row => row["COLUMN_NAME"].ToString()));
        }

        public IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");
            var tableName = string.IsNullOrEmpty(table.AliasedTable) ? table.ActualName : table.AliasedTable;
            var groups = GetForeignKeys(tableName)
                .Where(row =>
                    row["TABLE_SCHEMA"].ToString() == table.Schema && row["TABLE_NAME"].ToString() == tableName)
                .GroupBy(row => row["CONSTRAINT_NAME"].ToString())
                .ToList();

            foreach (var group in groups)
            {
                yield return new ForeignKey(new ObjectName(group.First()["TABLE_SCHEMA"].ToString(), group.First()["TABLE_NAME"].ToString()),
                    group.Select(row => row["COLUMN_NAME"].ToString()),
                    new ObjectName(group.First()["UNIQUE_TABLE_SCHEMA"].ToString(), group.First()["UNIQUE_TABLE_NAME"].ToString()),
                    group.Select(row => row["UNIQUE_COLUMN_NAME"].ToString()));
            }
        }

        public string QuoteObjectName(string unquotedName)
        {
            if (unquotedName == null) throw new ArgumentNullException("unquotedName");
            if (unquotedName.StartsWith("[")) return unquotedName;
            return string.Concat("[", unquotedName, "]");
        }

        public string NameParameter(string baseName)
        {
            if (baseName == null) throw new ArgumentNullException("baseName");
            if (baseName.Length == 0) throw new ArgumentException("Base name must be provided", "baseName");
            return (baseName.StartsWith("@")) ? baseName : "@" + baseName;
        }

        public Type DataTypeToClrType(string dataType)
        {
            return SqlTypeResolver.GetClrType(dataType);
        }

        private DataTable GetColumnsDataTable(Table table)
        {
            const string columnSelect = @"SELECT name, is_identity, type_name(system_type_id) as type_name, max_length from sys.columns 
                where object_id = object_id(@tableName, 'TABLE') or object_id = object_id(@tableName, 'VIEW') order by column_id";
            var @tableName = new SqlParameter("@tableName", SqlDbType.NVarChar, 128);
            @tableName.Value = string.Format("[{0}].[{1}]", table.Schema, table.ActualName);
            return SelectToDataTable(columnSelect, @tableName);
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
            var primaryKeys = GetPrimaryKeys();
            try
            {
                var dataTable =
                    primaryKeys.AsEnumerable()
                               .Where(
                                   row =>
                                   row["TABLE_NAME"].ToString()
                                                    .Equals(tableName, StringComparison.InvariantCultureIgnoreCase))
                               .CopyToDataTable();
                return dataTable;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private EnumerableRowCollection<DataRow> GetForeignKeys(string tableName)
        {
            return GetForeignKeys().AsEnumerable()
                .Where(
                    row => row["TABLE_NAME"].ToString().Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
        }

        private DataTable SelectToDataTable(string sql, params SqlParameter[] parameters)
        {
            var dataTable = new DataTable();
            using (var cn = ConnectionProvider.CreateConnection() as SqlConnection)
            {
                using (var adapter = new SqlDataAdapter(sql, cn))
                {
                    adapter.SelectCommand.Parameters.AddRange(parameters);
                    adapter.Fill(dataTable);
                }

            }

            return dataTable;
        }

        private static SqlDbType DbTypeFromInformationSchemaTypeName(string informationSchemaTypeName)
        {
            return DbTypeLookup.GetSqlDbType(informationSchemaTypeName);
        }

        public String GetDefaultSchema()
        {
            return "dbo";
        }

        private IEnumerable<Tuple<string, string>> getSynonyms()
        {
            const string synSelect = @"SELECT name, base_object_name FROM SYS.SYNONYMS";
            var dataTable = new DataTable();
            using (var cn = ConnectionProvider.CreateConnection() as SqlConnection)
            {
                using (var adapter = new SqlDataAdapter(synSelect, cn))
                {
                    adapter.Fill(dataTable);
                }

            }
            foreach (DataRow row in dataTable.Rows)
            {
                //get the object name with schema while removing the database name
                var objName = buildBaseObjectName(row["base_object_name"].ToString());
                yield return new Tuple<string, string>(row["name"].ToString(), objName);
            }
        }

        private string buildBaseObjectName(string baseObjectName)
        {

            if (baseObjectName.Contains('.'))
            {
                var objParts = baseObjectName.Split('.');
                baseObjectName = objParts[objParts.Length - 2] + "." + objParts[objParts.Length - 1];
            }

            return baseObjectName.Replace("[", "").Replace("]", "");
        }
    }
}
