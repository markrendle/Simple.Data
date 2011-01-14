using System.Collections.Generic;

namespace Simple.Data.Ado.Schema
{
    public sealed class ForeignKey
    {
        private readonly Key _columns;
        private readonly ObjectName _detailTable;
        private readonly ObjectName _masterTable;
        private readonly Key _masterColumns;

        public ForeignKey(ObjectName detailTable, IEnumerable<string> columns, ObjectName masterTable, IEnumerable<string> masterColumns)
        {
            _columns = new Key(columns);
            _detailTable = detailTable;
            _masterTable = masterTable;
            _masterColumns = new Key(masterColumns);
        }

        public ObjectName DetailTable
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

        public ObjectName MasterTable
        {
            get { return _masterTable; }
        }
    }
}
