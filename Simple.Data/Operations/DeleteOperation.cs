namespace Simple.Data.Operations
{
    public class DeleteOperation : IOperation
    {
        private readonly SimpleExpression _criteria;
        private readonly string _tableName;

        public DeleteOperation(string tableName, SimpleExpression criteria)
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