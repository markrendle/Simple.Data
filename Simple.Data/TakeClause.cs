namespace Simple.Data
{
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