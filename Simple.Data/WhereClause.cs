namespace Simple.Data
{
    public class WhereClause : SimpleQueryClauseBase
    {
        private readonly SimpleExpression _criteria;

        public WhereClause(SimpleExpression criteria)
        {
            _criteria = criteria;
        }

        public SimpleExpression Criteria
        {
            get { return _criteria; }
        }
    }
}