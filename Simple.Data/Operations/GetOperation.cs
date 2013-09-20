namespace Simple.Data.Operations
{
    public class GetOperation : IOperation
    {
        private readonly string _tableName;
        private readonly object[] _keyValues;

        public GetOperation(string tableName, object[] keyValues)
        {
            _tableName = tableName;
            _keyValues = keyValues;
        }

        public object[] KeyValues
        {
            get { return _keyValues; }
        }

        public string TableName
        {
            get { return _tableName; }
        }
    }
}