using Simple.Data.Operations;

namespace Simple.Data
{
    using System.Collections.Generic;
    using System.Linq;

    internal class DatabaseRunner : RunStrategy
    {
        private readonly Adapter _adapter;
        public DatabaseRunner(Adapter adapter)
        {
            _adapter = adapter;
        }

        protected override Adapter Adapter
        {
            get { return _adapter; }
        }

        internal override OperationResult Execute(IOperation operation)
        {
            return _adapter.Execute(operation);
        }
    }
}