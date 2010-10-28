using System.Collections.Generic;
using System.Data;

namespace Simple.Data.Ado.Schema
{
    public class Column
    {
        private readonly string _actualName;
        private readonly Table _table;
        private readonly bool _isIdentity;

        public Column(string actualName, Table table) : this(actualName, table, false)
        {
        }

        public Column(string actualName, Table table, bool isIdentity)
        {
            _actualName = actualName;
            _isIdentity = isIdentity;
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

        public bool IsIdentity
        {
            get { return _isIdentity; }
        }
    }
}
