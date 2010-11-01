using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.Ado.Schema
{
    public sealed class Key
    {
        private readonly string[] _columns;
        public Key(IEnumerable<string> columns)
        {
            _columns = columns.ToArray();
        }

        public string this[int index]
        {
            get { return _columns[index]; }
        }

        public int Length
        {
            get { return _columns.Length; }
        }

        public IEnumerable<string> AsEnumerable()
        {
            return _columns.AsEnumerable();
        }
    }
}
