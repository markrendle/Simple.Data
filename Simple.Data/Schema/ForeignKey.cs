using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Schema
{
    class ForeignKey
    {
        private readonly Key _columns;
        private readonly string _detailTable;
        private readonly string _masterTable;
        private readonly Key _masterColumns;

        public ForeignKey(string detailTable, string[] columns, string masterTable, string[] masterColumns)
        {
            _columns = new Key(columns);
            _detailTable = detailTable;
            _masterTable = masterTable;
            _masterColumns = new Key(masterColumns);
        }

        public string DetailTable
        {
            get { return _detailTable; }
        }

        public Key UniqueColumns
        {
            get { return _masterColumns; }
        }

        public Key Columns
        {
            get { return _columns; }
        }

        public string MasterTable
        {
            get { return _masterTable; }
        }
    }
}
