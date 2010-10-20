using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Ado;

namespace Simple.Data.Mocking.Ado
{
    public class MockSchemaProvider : ISchemaProvider
    {
        private static readonly IDictionary<string, DataTable> Tables = new Dictionary<string, DataTable>();

        public static void SetTables(params object[][] rows)
        {
            var table = new DataTable("TABLES");
            table.AddColumns("TABLE_SCHEMA", "TABLE_NAME", "TABLE_TYPE");
            table.AddRows(rows);
            Tables.Add("TABLES", table);
        }

        public static void SetColumns(params object[][] rows)
        {
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
            table.AddColumns("TABLE_SCHEMA", "TABLE_NAME", "COLUMN_NAME",
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
    }
}
