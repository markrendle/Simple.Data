using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

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
            return table.DatabaseSchema.Database.Query(
                "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = ?", table.ActualName)
                .Select(d => new Column(d.ColumnName.ToString()));
        }
    }
}
