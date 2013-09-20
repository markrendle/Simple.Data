namespace Simple.Data.Operations
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class InsertOperation
    {
        private readonly IEnumerable<IReadOnlyDictionary<string, object>> _data;
        private readonly bool _resultRequired;
        private readonly string _tableName;
        private readonly ErrorCallback _errorCallback;

        public InsertOperation(string tableName, IDictionary<string, object> data, bool resultRequired, ErrorCallback errorCallback = null)
            : this(tableName, resultRequired, errorCallback)
        {
            _data = EnumerableEx.Once(new ReadOnlyDictionary<string, object>(data)); 
        }

        public InsertOperation(string tableName, IReadOnlyDictionary<string, object> data, bool resultRequired, ErrorCallback errorCallback = null)
            : this(tableName, resultRequired, errorCallback)
        {
            _data = EnumerableEx.Once(data);
        }

        public InsertOperation(string tableName, IEnumerable<IDictionary<string, object>> data, bool resultRequired, ErrorCallback errorCallback = null)
            : this(tableName, resultRequired, errorCallback)
        {
            _data = data.Select(d => new ReadOnlyDictionary<string, object>(d));
        }

        public InsertOperation(string tableName, bool resultRequired, ErrorCallback errorCallback)
        {
            _tableName = tableName;
            _resultRequired = resultRequired;
            _errorCallback = errorCallback ?? ((item, exception) => true) ;
        }

        public InsertOperation(string tableName, IEnumerable<IReadOnlyDictionary<string, object>> data, bool resultRequired, ErrorCallback errorCallback = null)
            : this(tableName, resultRequired, errorCallback)
        {
            _data = data;
        }

        public bool ResultRequired
        {
            get { return _resultRequired; }
        }

        public string TableName
        {
            get { return _tableName; }
        }

        public IEnumerable<IReadOnlyDictionary<string, object>> Data
        {
            get { return _data; }
        }

        public ErrorCallback ErrorCallback
        {
            get { return _errorCallback; }
        }
    }
}