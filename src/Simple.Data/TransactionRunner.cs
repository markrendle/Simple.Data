using Simple.Data.Operations;

namespace Simple.Data
{
    using System.Collections.Generic;
    using System.Linq;

    internal class TransactionRunner : RunStrategy
    {
        private readonly IAdapterWithTransactions _adapter;
        private readonly IAdapterTransaction _adapterTransaction;

        public TransactionRunner(IAdapterWithTransactions adapter, IAdapterTransaction adapterTransaction)
        {
            _adapter = adapter;
            _adapterTransaction = adapterTransaction;
        }

        protected override Adapter Adapter
        {
            get { return (Adapter) _adapter; }
        }

        internal override OperationResult Execute(IOperation operation)
        {
            return _adapter.Execute(operation, _adapterTransaction);
        }
    }
}