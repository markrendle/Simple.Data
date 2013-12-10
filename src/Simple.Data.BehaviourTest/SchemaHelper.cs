using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Simple.Data.IntegrationTest
{
    class SchemaHelper
    {
        public static DataSet DummySchema()
        {
            var dataSet = new DataSet();

            dataSet.Tables.Add(CreateTables());
            dataSet.Tables.Add(CreateColumns());

            return dataSet;
        }

        private static DataTable CreateTables()
        {
            var tables = new DataTable("Tables");
            tables.Columns.Add("table_schema", typeof(string));
            tables.Columns.Add("table_name", typeof(string));
            tables.Columns.Add("table_type", typeof(string));
            tables.Rows.Add("dbo", "Customer", "BASE TABLE");
            tables.Rows.Add("dbo", "Orders", "BASE TABLE");
            return tables;
        }

        private static DataTable CreateColumns()
        {
            var columns = new DataTable("Columns");

            columns.Columns.Add("table_schema", typeof(string));
            columns.Columns.Add("table_name", typeof(string));
            columns.Columns.Add("column_name", typeof(string));
            columns.Columns.Add("is_nullable", typeof(string));
            columns.Columns.Add("data_type", typeof(string));
            columns.Columns.Add("character_maximum_length", typeof(int));

            columns.Rows.Add("dbo", "Customer", "CustomerId", "NO", "integer", DBNull.Value);
            columns.Rows.Add("dbo", "Customer", "Name", "NO", "varchar", 100);

            columns.Rows.Add("dbo", "Orders", "OrderId", "NO", "integer", DBNull.Value);
            columns.Rows.Add("dbo", "Orders", "CustomerId", "NO", "integer", DBNull.Value);
            columns.Rows.Add("dbo", "Orders", "Date", "NO", "datetime", DBNull.Value);

            return columns;
        }
    }
}
