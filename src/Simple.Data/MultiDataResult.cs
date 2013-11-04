namespace Simple.Data
{
    using System.Collections.Generic;

    public sealed class MultiDataResult : OperationResult
    {
        private readonly IEnumerable<IEnumerable<IDictionary<string, object>>> _results;

        public MultiDataResult(IEnumerable<IEnumerable<IDictionary<string, object>>> results) : base(0)
        {
            _results = results;
        }

        public IEnumerable<IEnumerable<IDictionary<string, object>>> Results
        {
            get { return _results; }
        }
    }
}