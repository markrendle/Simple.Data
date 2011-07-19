namespace Simple.Data
{
    public class HavingClause : SimpleQueryClauseBase
    {
        private readonly SimpleExpression _criteria;

        public HavingClause(SimpleExpression criteria)
        {
            _criteria = criteria;
        }

        public SimpleExpression Criteria
        {
            get { return _criteria; }
        }
    }
}