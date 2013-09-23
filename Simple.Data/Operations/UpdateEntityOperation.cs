namespace Simple.Data.Operations
{
    using System.Collections.Generic;

    public class UpdateEntityOperation : IOperation
    {
        private readonly string _tableName;
        private readonly IEnumerable<IReadOnlyDictionary<string, object>> _data;
        private readonly IEnumerable<IReadOnlyDictionary<string, object>> _originals;

        public UpdateEntityOperation(string tableName, IReadOnlyDictionary<string, object> data, IReadOnlyDictionary<string, object> original = null)
            : this(tableName, EnumerableEx.Once(data), original == null ? null : EnumerableEx.Once(original))
        {
            
        }

        public UpdateEntityOperation(string tableName, IEnumerable<IReadOnlyDictionary<string, object>> data, IEnumerable<IReadOnlyDictionary<string, object>> originals = null)
        {
            _tableName = tableName;
            _data = data;
            _originals = originals;
        }

        public string TableName
        {
            get { return _tableName; }
        }

        public IEnumerable<IReadOnlyDictionary<string, object>> Data
        {
            get { return _data; }
        }

        public IEnumerable<IReadOnlyDictionary<string, object>> Originals
        {
            get { return _originals; }
        }
    }
}