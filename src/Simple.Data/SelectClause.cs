using System.Collections.Generic;

namespace Simple.Data
{
    /// <summary>
    /// Represents the set of columns/fields to be returned by a <see cref="SimpleQuery"/>.
    /// There will be at most one instance of this type in <see cref="SimpleQuery.Clauses"/>.
    /// </summary>
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