using System.Collections.Generic;
using System.Data;

namespace Simple.Data.Ado.Schema
{
    class Column
    {
        private readonly string _actualName;
        private readonly Table _table;
        private readonly string _homogenizedName;

        public Column(string actualName, Table table)
        {
            _actualName = actualName;
            _table = table;
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

        public string QuotedName
        {
            get { return _table.DatabaseSchema.QuoteObjectName(_actualName); }
        }

        public static IEnumerable<Column> GetColumnsForTable(Table table)
        {
            var columns = table.DatabaseSchema.SchemaProvider.GetSchema("COLUMNS", null, table.Schema, table.ActualName);

            return columns.AsEnumerable().Select(row => new Column(row["COLUMN_NAME"].ToString(), table));
        }
    }
}
