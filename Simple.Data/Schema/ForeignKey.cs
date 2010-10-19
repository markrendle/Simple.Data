using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Schema
{
    class ForeignKey
    {
        private readonly Key _columns;
        private readonly string _uniqueTable;
        private readonly Key _uniqueColumns;

        public ForeignKey(string[] columns, string uniqueTable, string[] uniqueColumns)
        {
            _columns = new Key(columns);
            _uniqueTable = uniqueTable;
            _uniqueColumns = new Key(uniqueColumns);
        }

        public Key UniqueColumns
        {
            get { return _uniqueColumns; }
        }

        public Key Columns
        {
            get { return _columns; }
        }

        public string UniqueTable
        {
            get { return _uniqueTable; }
        }
    }
}
