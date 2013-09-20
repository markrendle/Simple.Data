namespace Simple.Data.Operations
{
    public class FindOperation : IOperation
    {
        private readonly string _tableName;
        private readonly SimpleExpression _criteria;

        public FindOperation(string tableName, SimpleExpression criteria)
        {
            _tableName = tableName;
            _criteria = criteria;
        }

        public SimpleExpression Criteria
        {
            get { return _criteria; }
        }

        public string TableName
        {
            get { return _tableName; }
        }
    }
}