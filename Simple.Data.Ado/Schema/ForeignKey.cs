using System.Collections.Generic;

namespace Simple.Data.Ado.Schema
{
    public sealed class ForeignKey
    {
        private readonly Key _columns;
        private readonly ObjectName _detailTable;
        private readonly ObjectName _masterTable;
        private readonly string _name;
        private readonly Key _masterColumns;

        public ForeignKey(ObjectName detailTable, IEnumerable<string> columns, ObjectName masterTable, IEnumerable<string> masterColumns) : this(detailTable, columns, masterTable, masterColumns, null)
        {
        }

        public ForeignKey(ObjectName detailTable, IEnumerable<string> columns, ObjectName masterTable, IEnumerable<string> masterColumns, string name)
        {
            _columns = new Key(columns);
            _detailTable = detailTable;
            _masterTable = masterTable;
            _name = name;
            _masterColumns = new Key(masterColumns);
        }

        public string Name
        {
            get { return _name; }
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
