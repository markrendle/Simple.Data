namespace Simple.Data
{
    public class SkipClause : SimpleQueryClauseBase
    {
        private readonly int _count;

        public SkipClause(int count)
        {
            _count = count;
        }

        public int Count
        {
            get { return _count; }
        }
    }
}