using System.Collections.Generic;
using System.Data;

namespace Simple.Data.Ado.Schema
{
    public class Column
    {
        private readonly string _actualName;
        private readonly Table _table;

        public Column(string actualName, Table table)
        {
            _actualName = actualName;
            _table = table;
        }

        public string HomogenizedName
        {
            get { return ActualName.Homogenize(); }
        }

        public string ActualName
        {
            get { return _actualName; }
        }

        public string QuotedName
        {
            get { return _table.DatabaseSchema.QuoteObjectName(_actualName); }
        }
    }
}
