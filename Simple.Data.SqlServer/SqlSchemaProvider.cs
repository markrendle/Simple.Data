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
        enum ObjectNamePart
        {
            Name = 1,
            Schema = 2,
            Database = 3,
            Server = 4
        }

        private readonly IConnectionProvider _connectionProvider;
        private IEnumerable<Tuple<string, string, string>> _synonyms;

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
            foreach (var syn in _synonyms.Where(s => s.Item3 == "U" || s.Item3 == "V"))
                synTables.Add(new Table(syn.Item1, getObjectPart(syn.Item2, ObjectNamePart.Schema)
                    , syn.Item3 == "U" ? TableType.Table : TableType.View
                    , getObjectPart(syn.Item2, ObjectNamePart.Name)));

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
            //if this table is a synonym, get columns from real table
            if (!string.IsNullOrEmpty(table.AliasedTable))
                return GetColumnsFromSchemaQuery(table).AsEnumerable().Select(row => SynonymSchemaRowToColumn(table, row));
            else
                return GetColumnsDataTable(table).AsEnumerable().Select(row => SchemaRowToColumn(table, row));
        }

        private static Column SynonymSchemaRowToColumn(Table table, DataRow row)
        {
            SqlDbType sqlDbType = SqlDbType.Udt;

            if (!row.IsNull("DataTypeName"))
                sqlDbType = DbTypeFromInformationSchemaTypeName((string)row["DataTypeName"]);

            var size = (int)row["ColumnSize"];
            switch (sqlDbType)
            {
                case SqlDbType.Image:
                case SqlDbType.NText:
                case SqlDbType.Text:
                    size = -1;
                    break;
            }

            return new SqlColumn(row["ColumnName"].ToString(), table, (bool)row["IsIdentity"], sqlDbType, size);
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
            foreach (var syn in _synonyms.Where(s => s.Item3 == "P"))
                synProcs.Add(new Procedure(syn.Item1, syn.Item1, string.Empty, syn.Item2));

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
                : storedProcedure.AliasedProcedure;
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
            var tableName = table.ActualName;
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
            var tableName = table.ActualName;
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

        private DataTable GetColumnsFromSchemaQuery(Table table)
        {
            DataTable dt = new DataTable();
            using (var cn = ConnectionProvider.CreateConnection() as SqlConnection)
            {
                string commandText = string.Format("select top 1 * from {0}", table.QualifiedName);
                using (SqlCommand cmd = new SqlCommand(commandText, cn))
                {
                    cmd.CommandType = CommandType.Text;
                    cn.Open();
                    SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    return dr.GetSchemaTable();
                }
            }
        }

        private DataTable GetPrimaryKeys()
        {
            var dt = SelectToDataTable(Properties.Resources.PrimaryKeySql);
            //get pks for synonym tables
            foreach (var syn in _synonyms.Where(s => s.Item3 == "U" || s.Item3 == "V"))
            {
                DataTable remoteTable = GetRemotePrimaryKey(syn.Item1, syn.Item2);
                if (remoteTable.Rows.Count > 0)
                    dt.Merge(remoteTable);
            }
            return dt;
        }

        private DataTable GetRemotePrimaryKey(string tableName, string remoteTableName)
        {
            return SelectToDataTable(string.Format(Properties.Resources.RemotePrimaryKeySql
                , tableName
                , getObjectPart(remoteTableName, ObjectNamePart.Server) + "." + getObjectPart(remoteTableName, ObjectNamePart.Database) + "."
                , getObjectPart(remoteTableName, ObjectNamePart.Name)));
        }

        private DataTable GetForeignKeys()
        {
            var dt = SelectToDataTable(Properties.Resources.ForeignKeysSql);
            //get fks for synonym tables
            foreach (var syn in _synonyms.Where(s => s.Item3 == "U" || s.Item3 == "V"))
            {
                DataTable remoteTable = GetRemoteForeignKeys(syn.Item1, syn.Item2);
                if (remoteTable.Rows.Count > 0)
                    dt.Merge(remoteTable);
            }
            return dt;
        }

        private DataTable GetRemoteForeignKeys(string tableName, string remoteTableName)
        {
            var serverName = getObjectPart(remoteTableName, ObjectNamePart.Server);
            var databaseName = getObjectPart(remoteTableName, ObjectNamePart.Database);
            return SelectToDataTable(string.Format(Properties.Resources.RemoteForeignKeysSql
               , tableName
               , serverName + "." + databaseName + "."
               , "[" + serverName + "].[" + databaseName + "]."
               , getObjectPart(remoteTableName, ObjectNamePart.Name)));
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

        private IEnumerable<Tuple<string, string, string>> getSynonyms()
        {
            const string synSelect = @"SELECT name, base_object_name, OBJECTPROPERTYEX(OBJECT_ID(name), 'BaseType') AS BaseType FROM SYS.SYNONYMS;";
            var dataTable = new DataTable();
            using (var cn = ConnectionProvider.CreateConnection() as SqlConnection)
            {
                using (var adapter = new SqlDataAdapter(synSelect, cn))
                {
                    adapter.Fill(dataTable);
                }
            }
            foreach (DataRow row in dataTable.Rows)
                yield return new Tuple<string, string, string>(row["name"].ToString(), row["base_object_name"].ToString(), row["BaseType"].ToString().Trim());
        }

        private string getObjectPart(string fullObjectName, ObjectNamePart objectNamePart)
        {
            if (!fullObjectName.Contains('.'))
                return fullObjectName;

            var objParts = fullObjectName.Split('.');
            if (objParts.Length < (int)objectNamePart)
            {
                if (objectNamePart == ObjectNamePart.Schema)
                    return GetDefaultSchema();
                else
                    return string.Empty;
            }
            string result = objParts[objParts.Length - (int)objectNamePart];
            return result.Replace("[", string.Empty).Replace("]", string.Empty);
        }
    }
}
