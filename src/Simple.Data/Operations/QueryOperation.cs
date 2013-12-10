namespace Simple.Data.Operations
{
    public class QueryOperation : IOperation
    {
        private readonly SimpleQuery _query;

        internal QueryOperation(DataStrategy dataStrategy, string tableName, SimpleExpression criteria)
        {
            _query = new SimpleQuery(dataStrategy, tableName).Where(criteria);
        }

        internal QueryOperation(SimpleQuery query)
        {
            _query = query;
        }

        public SimpleQuery Query
        {
            get { return _query; }
        }
    }
}