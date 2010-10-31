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
        [ThreadStatic]
        private static IDictionary<string, DataTable> _tables;

        private static IDictionary<string, DataTable> Tables
        {
            get { return _tables ?? (_tables = new Dictionary<string, DataTable>()); }
        }

        public static void SetTables(params object[][] rows)
        {
            Tables.Remove("TABLES");
            var table = new DataTable("TABLES");
            table.AddColumns("TABLE_SCHEMA", "TABLE_NAME", "TABLE_TYPE");
            table.AddRows(rows);
            Tables.Add("TABLES", table);
        }

        public static void SetColumns(params object[][] rows)
        {
            Tables.Remove("COLUMNS");
            var table = new DataTable("COLUMNS");
            table.AddColumns("TABLE_SCHEMA", "TABLE_NAME", "COLUMN_NAME");
            table.AddRows(rows);
            Tables.Add("COLUMNS", table);
        }

        public static void SetPrimaryKeys(params object[][] rows)
        {
            var table = new DataTable("PRIMARY_KEYS");
            table.AddColumns("TABLE_SCHEMA", "TABLE_NAME", "COLUMN_NAME");
            table.Columns.Add("ORDINAL_POSITION", typeof (int));
            table.AddRows(rows);
            Tables.Add("PRIMARY_KEYS", table);
        }

        public static void SetForeignKeys(params object[][] rows)
        {
            var table = new DataTable("FOREIGN_KEYS");
            table.AddColumns("CONSTRAINT_NAME", "TABLE_SCHEMA", "TABLE_NAME", "COLUMN_NAME",
                             "UNIQUE_TABLE_SCHEMA", "UNIQUE_TABLE_NAME", "UNIQUE_COLUMN_NAME");
            table.Columns.Add("ORDINAL_POSITION", typeof(int));
            table.AddRows(rows);
            Tables.Add("FOREIGN_KEYS", table);
        }

        public DataTable GetSchema(string collectionName)
        {
            return Tables[collectionName];
        }

        public DataTable GetSchema(string collectionName, params string[] restrictionValues)
        {
            return Tables[collectionName];
        }

        public IEnumerable<Table> GetTables()
        {
            return Tables["TABLES"].AsEnumerable()
                .Select(row => new Table(row["TABLE_NAME"].ToString(), row["TABLE_SCHEMA"].ToString(), TableType.Table));
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            return Tables["COLUMNS"].AsEnumerable()
                .Where(row => row["TABLE_SCHEMA"].ToString() == table.Schema && row["TABLE_NAME"].ToString() == table.ActualName)
                .Select(row => new Column(row["COLUMN_NAME"].ToString(), table));
        }

        public Key GetPrimaryKey(Table table)
        {
            return new Key(Tables["PRIMARY_KEYS"].AsEnumerable()
                .Where(
                    row =>
                    row["TABLE_SCHEMA"].ToString() == table.Schema && row["TABLE_NAME"].ToString() == table.ActualName)
                    .OrderBy(row => (int)row["ORDINAL_POSITION"])
                    .Select(row => row["COLUMN_NAME"].ToString()));
        }

        public IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            var groups = Tables["FOREIGN_KEYS"].AsEnumerable()
                .Where(row =>
                    row["TABLE_SCHEMA"].ToString() == table.Schema && row["TABLE_NAME"].ToString() == table.ActualName)
                .GroupBy(row => row["CONSTRAINT_NAME"].ToString())
                .ToList();

            foreach (var group in groups)
            {
                yield return new ForeignKey(group.First()["TABLE_NAME"].ToString(),
                    group.Select(row => row["COLUMN_NAME"].ToString()),
                    group.First()["UNIQUE_TABLE_NAME"].ToString(),
                    group.Select(row => row["UNIQUE_COLUMN_NAME"].ToString()));
            }
        }

        public static void Reset()
        {
            Tables.Clear();
        }
    }
}
