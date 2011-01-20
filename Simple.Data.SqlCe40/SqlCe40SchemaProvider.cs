using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.SqlCe40
{
    class SqlCe40SchemaProvider : ISchemaProvider
    {
        private readonly IConnectionProvider _connectionProvider;

        public SqlCe40SchemaProvider(IConnectionProvider connectionProvider)
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
            return GetColumnsDataTable(table).AsEnumerable().Select(row => SchemaRowToColumn(table, row));
        }

        private static Column SchemaRowToColumn(Table table, DataRow row)
        {
            return new Column(row["name"].ToString(), table, (bool)row["is_identity"]);
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
            return GetSchema("ProcedureParameters", null, storedProcedure.Schema, storedProcedure.SpecificName)
                .Select(SchemaRowToProcedureParameter);
        }

        private static Parameter SchemaRowToProcedureParameter(DataRow row)
        {
            return new Parameter(row["parameter_name"].ToString(), SqlTypeResolver.GetClrType(row["data_type"].ToString()),
                DirectionFromString(row["parameter_mode"].ToString()));
        }

        private static ParameterDirection DirectionFromString(string mode)
        {
            switch (mode)
            {
                case "IN":
                    return ParameterDirection.Input;
                case "OUT":
                    return ParameterDirection.Output;
                default:
                    return ParameterDirection.InputOutput;
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

        public Type DataTypeToClrType(string dataType)
        {
            return SqlTypeResolver.GetClrType(dataType);
        }

        private DataTable GetColumnsDataTable(Table table)
        {
            var columnSelect =
                string.Format(
                    "SELECT name, is_identity from sys.columns where object_id = object_id('{0}.{1}', 'TABLE')",
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
            using (var cn = ConnectionProvider.CreateConnection() as SqlCeConnection)
            {
                using (var adapter = new SqlCeDataAdapter(sql, cn))
                {
                    adapter.Fill(dataTable);
                }

            }

            return dataTable;
        }
    }
}
