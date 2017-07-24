namespace Shitty.Data
{
    /// <summary>
    /// Represents the filtering criteria for a <see cref="SimpleQuery"/>.
    /// There may be zero, one or multiple instances of this type in <see cref="SimpleQuery.Clauses"/>;
    /// for multiple instances, criteria should be combined with the AND operator.
    /// </summary>
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