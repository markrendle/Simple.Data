namespace Simple.Data.Operations
{
    using System.Collections.Generic;

    public class UpdateByCriteriaOperation : IOperation
    {
        private readonly string _tableName;
        private readonly SimpleExpression _criteria;
        private readonly IReadOnlyDictionary<string, object> _data;

        public string TableName
        {
            get { return _tableName; }
        }

        public SimpleExpression Criteria
        {
            get { return _criteria; }
        }

        public IReadOnlyDictionary<string, object> Data
        {
            get { return _data; }
        }

        public UpdateByCriteriaOperation(string tableName, SimpleExpression criteria, IReadOnlyDictionary<string, object> data)
        {
            _tableName = tableName;
            _criteria = criteria;
            _data = data;
        }
    }
}