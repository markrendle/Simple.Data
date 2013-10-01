namespace Simple.Data.Operations
{
    using System.Collections.Generic;

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

    public class FunctionOperation : IOperation
    {
        private readonly string _functionName;
        private readonly IDictionary<string, object> _parameters;

        public string FunctionName
        {
            get { return _functionName; }
        }

        public IDictionary<string, object> Parameters
        {
            get { return _parameters; }
        }

        public FunctionOperation(string functionName, IDictionary<string, object> parameters)
        {
            _functionName = functionName;
            _parameters = parameters;
        }
    }
}