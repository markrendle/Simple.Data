using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Simple.Data.Schema
{
    class Column
    {
        private readonly string _name;

        public Column(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public static IEnumerable<Column> GetColumnsForTable(Table table)
        {
            return table.DatabaseSchema.Database.Query(
                "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = ?", table.Name)
                .Select(d => new Column(d.COLUMN_NAME.ToString()));
        }
    }
}
