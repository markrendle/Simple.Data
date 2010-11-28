using System.Collections.Generic;

namespace Simple.Data.Ado.Schema
{
    public sealed class ForeignKey
    {
        private readonly Key _columns;
        private readonly TableName _detailTable;
        private readonly TableName _masterTable;
        private readonly Key _masterColumns;

        public ForeignKey(TableName detailTable, IEnumerable<string> columns, TableName masterTable, IEnumerable<string> masterColumns)
        {
            _columns = new Key(columns);
            _detailTable = detailTable;
            _masterTable = masterTable;
            _masterColumns = new Key(masterColumns);
        }

        public TableName DetailTable
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

        public TableName MasterTable
        {
            get { return _masterTable; }
        }
    }
}
