namespace Simple.Data
{
    using System.Collections.Generic;
    using System.Linq;

    public sealed class QueryResult : OperationResult
    {
        private readonly IEnumerable<IDictionary<string, object>> _data;
        private readonly IEnumerable<SimpleQueryClauseBase> _unhandledClauses;

        public IEnumerable<IDictionary<string, object>> Data
        {
            get { return _data; }
        }

        public IEnumerable<SimpleQueryClauseBase> UnhandledClauses
        {
            get { return _unhandledClauses; }
        }

        public QueryResult(IEnumerable<IDictionary<string, object>> data, IEnumerable<SimpleQueryClauseBase> unhandledClauses = null) : base(0)
        {
            _data = data;
            _unhandledClauses = unhandledClauses ?? Enumerable.Empty<SimpleQueryClauseBase>();
        }
    }
}