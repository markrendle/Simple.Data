namespace Shitty.Data
{
    /// <summary>
    /// Represents a number of records to take from the results returned by a <see cref="SimpleQuery"/>.
    /// There will be at most one instance of this type in <see cref="SimpleQuery.Clauses"/>.
    /// </summary>
    public class TakeClause : SimpleQueryClauseBase
    {
        private readonly int _count;

        public TakeClause(int count)
        {
            _count = count;
        }

        public int Count
        {
            get { return _count; }
        }
    }
}