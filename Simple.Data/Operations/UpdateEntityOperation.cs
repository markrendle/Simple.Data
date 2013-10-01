namespace Simple.Data.Operations
{
    using System.Collections.Generic;
    using System.Linq;

    public class UpdateEntityOperation : IOperation
    {
        private readonly string _tableName;
        private readonly IEnumerable<IReadOnlyDictionary<string, object>> _data;
        private readonly IEnumerable<IReadOnlyDictionary<string, object>> _originals;
        private readonly string[] _criteriaFieldNames;

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
        
        public UpdateEntityOperation(string tableName, IReadOnlyDictionary<string, object> data, IEnumerable<string> criteriaFieldNames)
            : this(tableName, EnumerableEx.Once(data), criteriaFieldNames)
        {
        }

        public UpdateEntityOperation(string tableName, IEnumerable<IReadOnlyDictionary<string, object>> data, IEnumerable<string> criteriaFieldNames)
        {
            _tableName = tableName;
            _data = data;
            _criteriaFieldNames = criteriaFieldNames.ToArray();
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

        public string[] CriteriaFieldNames
        {
            get { return _criteriaFieldNames; }
        }
    }
}