namespace Simple.Data.Ado.Schema
{
    class TableJoin
    {
        private readonly Table _master;
        private readonly Column _masterColumn;
        private readonly Table _detail;
        private readonly Column _detailColumn;

        public TableJoin(Table master, Column masterColumn, Table detail, Column detailColumn)
        {
            _master = master;
            _masterColumn = masterColumn;
            _detailColumn = detailColumn;
            _detail = detail;
        }

        public Column MasterColumn
        {
            get { return _masterColumn; }
        }

        public Table Detail
        {
            get { return _detail; }
        }

        public Table Master
        {
            get { return _master; }
        }

        public Column DetailColumn
        {
            get {
                return _detailColumn;
            }
        }
    }
}
