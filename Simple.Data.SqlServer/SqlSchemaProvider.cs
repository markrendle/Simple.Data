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
        // More info : http://msdn.microsoft.com/en-us/library/ms131092.aspx
        private static readonly Dictionary<string, DbType> DbTypeLookup = new Dictionary<string, DbType>
                                                                              {
                                                                                  {"bigint", DbType.Int64},
                                                                                  {"binary", DbType.Binary},
                                                                                  {"bit", DbType.Boolean},
                                                                                  {"char", DbType.AnsiStringFixedLength},
                                                                                  {"date", DbType.DateTime},
                                                                                  {"text", DbType.AnsiString},
                                                                                  {"uniqueidentifier", DbType.Guid},
                                                                                  {"time", DbType.DateTime},
                                                                                  {"datetime2", DbType.DateTime2},
                                                                                  {"datetimeoffset", DbType.DateTimeOffset},
                                                                                  {"tinyint", DbType.Byte},
                                                                                  {"smallint", DbType.Int16},
                                                                                  {"int", DbType.Int32},
                                                                                  {"smalldatetime", DbType.DateTime},
                                                                                  {"real", DbType.Single},
                                                                                  {"money", DbType.Currency},
                                                                                  {"datetime", DbType.DateTime},
                                                                                  {"float", DbType.Double},
                                                                                  {"sql_variant", DbType.Object},
                                                                                  {"ntext", DbType.String},
                                                                                  {"decimal", DbType.Decimal},
                                                                                  {"numeric", DbType.Decimal},
                                                                                  {"smallmoney", DbType.Currency},
                                                                                  {"varbinary", DbType.Binary},
                                                                                  {"varchar", DbType.AnsiString},
                                                                                  {"timestamp", DbType.Binary},
                                                                                  {"image", DbType.Binary},
                                                                                  {"nvarchar", DbType.String},
                                                                                  {"nchar", DbType.StringFixedLength},
                                                                                  {"xml", DbType.Xml},
                                                                              };
        private readonly IConnectionProvider _connectionProvider;

        public SqlSchemaProvider(IConnectionProvider connectionProvider)
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
            return GetSchema("TABLES").Select(SchemaRowToTable);
        }

        private static Table SchemaRowToTable(DataRow row)
        {
            return new Table(row["TABLE_NAME"].ToString(), row["TABLE_SCHEMA"].ToString(),
                        row["TABLE_TYPE"].ToString() == "BASE TABLE" ? TableType.Table : TableType.View);
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");
            var cols = GetColumnsDataTable(table);
            return cols.AsEnumerable().Select(row => SchemaRowToColumn(table, row));
        }

        private static Column SchemaRowToColumn(Table table, DataRow row)
        {
            return new Column(row["name"].ToString(), table, (bool) row["is_identity"],
                              DbTypeFromInformationSchemaTypeName((string) row["type_name"]), (short) row["max_length"]);
        }

        public IEnumerable<Procedure> GetStoredProcedures()
        {
            return GetSchema("Procedures").Select(SchemaRowToStoredProcedure);
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
            // GetSchema does not return the return value of e.g. a stored proc correctly,
            // i.e. there isn't sufficient information to correctly set up a stored proc.
            using (var connection = (SqlConnection)ConnectionProvider.CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = storedProcedure.SpecificName;

                    connection.Open();
                    SqlCommandBuilder.DeriveParameters(command);

                    foreach (SqlParameter p in command.Parameters)
                        yield return new Parameter(p.ParameterName, SqlTypeResolver.GetClrType(p.DbType.ToString()), p.Direction);
                }
            }
        }

        public Key GetPrimaryKey(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");
            return new Key(GetPrimaryKeys(table.ActualName).AsEnumerable()
                .Where(
                    row =>
                    row["TABLE_SCHEMA"].ToString() == table.Schema && row["TABLE_NAME"].ToString() == table.ActualName)
                    .OrderBy(row => (int)row["ORDINAL_POSITION"])
                    .Select(row => row["COLUMN_NAME"].ToString()));
        }

        public IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");
            var groups = GetForeignKeys(table.ActualName)
                .Where(row =>
                    row["TABLE_SCHEMA"].ToString() == table.Schema && row["TABLE_NAME"].ToString() == table.ActualName)
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
            if (baseName.Length == 0) throw new ArgumentException("Base name must be provided");
            return (baseName.StartsWith("@")) ? baseName : "@" + baseName;
        }

        public Type DataTypeToClrType(string dataType)
        {
            return SqlTypeResolver.GetClrType(dataType);
        }

        private DataTable GetColumnsDataTable(Table table)
        {
            var columnSelect =
                string.Format(
                    "SELECT name, is_identity, type_name(system_type_id) as type_name, max_length from sys.columns where object_id = object_id('{0}.{1}', 'TABLE') or object_id = object_id('{0}.{1}', 'VIEW') order by column_id",
                    table.Schema, table.ActualName);
            return SelectToDataTable(columnSelect);
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

        private EnumerableRowCollection<DataRow> GetForeignKeys(string tableName)
        {
            return GetForeignKeys().AsEnumerable()
                .Where(
                    row => row["TABLE_NAME"].ToString().Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
        }

        private DataTable SelectToDataTable(string sql)
        {
            var dataTable = new DataTable();
            using (var cn = ConnectionProvider.CreateConnection() as SqlConnection)
            {
                using (var adapter = new SqlDataAdapter(sql, cn))
                {
                    adapter.Fill(dataTable);
                }

            }

            return dataTable;
        }

        private static DbType DbTypeFromInformationSchemaTypeName(string informationSchemaTypeName)
        {
            return DbTypeLookup[informationSchemaTypeName];
        }
    }
}
