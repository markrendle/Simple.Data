using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Ado;

namespace Simple.Data.Schema
{
    class Column
    {
        private readonly string _actualName;
        private readonly string _homogenizedName;

        public Column(string actualName)
        {
            _actualName = actualName;
            _homogenizedName = actualName.Homogenize();
        }

        public string HomogenizedName
        {
            get { return _homogenizedName; }
        }

        public string ActualName
        {
            get { return _actualName; }
        }

        public static IEnumerable<Column> GetColumnsForTable(Table table)
        {
            var columns = table.DatabaseSchema.SchemaProvider.GetSchema("COLUMNS", null, table.Schema, table.ActualName);

            return columns.AsEnumerable().Select(row => new Column(row["COLUMN_NAME"].ToString()));
        }
    }
}
