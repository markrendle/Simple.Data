namespace Simple.Data
{
    using System.Collections.Generic;
    using System.Linq;

    public sealed class DataResult : OperationResult
    {
        public static readonly DataResult Empty = new DataResult(Enumerable.Empty<IDictionary<string,object>>());

        private readonly IEnumerable<IDictionary<string, object>> _data;

        public IEnumerable<IDictionary<string, object>> Data
        {
            get { return _data; }
        }

        public DataResult(IDictionary<string, object> data) : this(EnumerableEx.Once(data))
        {
            
        }

        public DataResult(IEnumerable<IDictionary<string, object>> data) : base(0)
        {
            _data = data;
        }
    }
}