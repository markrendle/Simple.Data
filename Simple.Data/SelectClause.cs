using System.Collections.Generic;

namespace Simple.Data
{
    public class SelectClause : SimpleQueryClauseBase
    {
        private readonly IEnumerable<SimpleReference> _columns;

        public SelectClause(IEnumerable<SimpleReference> columns)
        {
            _columns = columns;
        }

        public IEnumerable<SimpleReference> Columns
        {
            get { return _columns; }
        }
    }
}