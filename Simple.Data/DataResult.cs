namespace Simple.Data
{
    using System.Collections.Generic;

    public sealed class DataResult : OperationResult
    {
        private readonly IEnumerable<IDictionary<string, object>> _data;

        public IEnumerable<IDictionary<string, object>> Data
        {
            get { return _data; }
        }

        public DataResult(IEnumerable<IDictionary<string, object>> data) : base(0)
        {
            _data = data;
        }
    }
}