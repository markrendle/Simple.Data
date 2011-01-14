using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Mocking.Ado
{
    public class MockSchemaProvider : ISchemaProvider
    {
        private readonly IDictionary<string, DataTable> _tables = new Dictionary<string, DataTable>();

        public void SetTables(params object[][] rows)
        {
            _tables.Remove("TABLES");
            var table = new DataTable("TABLES");
            table.AddColumns("TABLE_SCHEMA", "TABLE_NAME", "TABLE_TYPE");
            table.AddRows(rows);
            _tables.Add("TABLES", table);
        }

        public void SetColumns(params object[][] rows)
        {
            _tables.Remove("COLUMNS");
            var table = new DataTable("COLUMNS");
            table.AddColumns("TABLE_SCHEMA", "TABLE_NAME", "COLUMN_NAME");
            table.AddRows(rows);
            _tables.Add("COLUMNS", table);
        }

        public void SetPrimaryKeys(params object[][] rows)
        {
            var table = new DataTable("PRIMARY_KEYS");
            table.AddColumns("TABLE_SCHEMA", "TABLE_NAME", "COLUMN_NAME");
            table.Columns.Add("ORDINAL_POSITION", typeof (int));
            table.AddRows(rows);
            _tables.Add("PRIMARY_KEYS", table);
        }

        public void SetForeignKeys(params object[][] rows)
        {
            var table = new DataTable("FOREIGN_KEYS");
            table.AddColumns("CONSTRAINT_NAME", "TABLE_SCHEMA", "TABLE_NAME", "COLUMN_NAME",
                             "UNIQUE_TABLE_SCHEMA", "UNIQUE_TABLE_NAME", "UNIQUE_COLUMN_NAME");
            table.Columns.Add("ORDINAL_POSITION", typeof(int));
            table.AddRows(rows);
            _tables.Add("FOREIGN_KEYS", table);
        }

        public DataTable GetSchema(string collectionName)
        {
            return _tables[collectionName];
        }

        public DataTable GetSchema(string collectionName, params string[] restrictionValues)
        {
            return _tables[collectionName];
        }

        public IEnumerable<Table> GetTables()
        {
            return _tables["TABLES"].AsEnumerable()
                .Select(row => new Table(row["TABLE_NAME"].ToString(), row["TABLE_SCHEMA"].ToString(), TableType.Table));
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            return _tables["COLUMNS"].AsEnumerable()
                .Where(row => row["TABLE_SCHEMA"].ToString() == table.Schema && row["TABLE_NAME"].ToString() == table.ActualName)
                .Select(row => new Column(row["COLUMN_NAME"].ToString(), table));
        }

        public IEnumerable<StoredProcedure> GetStoredProcedures()
        {
            return _tables["PROCEDURES"].AsEnumerable()
                .Select(row => new StoredProcedure(row["PROCEDURE_NAME"].ToString(), row["PROCEDURE_NAME"].ToString(), row["PROCEDURE_SCHEMA"].ToString()));
        }

        public IEnumerable<Parameter> GetParameters(StoredProcedure storedProcedure)
        {
            return _tables["PARAMETERS"].AsEnumerable()
                .Where(row => row["PROCEDURE_SCHEMA"].ToString() == storedProcedure.Schema && row["PROCEDURE_NAME"].ToString() == storedProcedure.Name)
                .Select(row => new Parameter(row["PARAMETER_NAME"].ToString(), typeof(object), ParameterDirection.Input));
        }

        public Key GetPrimaryKey(Table table)
        {
            return new Key(_tables["PRIMARY_KEYS"].AsEnumerable()
                .Where(
                    row =>
                    row["TABLE_SCHEMA"].ToString() == table.Schema && row["TABLE_NAME"].ToString() == table.ActualName)
                    .OrderBy(row => (int)row["ORDINAL_POSITION"])
                    .Select(row => row["COLUMN_NAME"].ToString()));
        }

        public IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            var groups = _tables["FOREIGN_KEYS"].AsEnumerable()
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

        public void Reset()
        {
            _tables.Clear();
        }
    }
}
