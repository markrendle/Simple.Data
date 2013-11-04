namespace Simple.Data
{
    public class ForUpdateClause : SimpleQueryClauseBase
    {
        public bool SkipLockedRows { get; private set; }

        public ForUpdateClause(bool skipLockedRows)
        {
            SkipLockedRows = skipLockedRows;
        }
    }
}
