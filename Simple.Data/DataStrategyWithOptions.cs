using System.Data;
using Shitty.Data.Commands;

namespace Shitty.Data
{
    internal class DataStrategyWithOptions : DataStrategy
    {
        private readonly DataStrategy _wrappedStrategy;
        private readonly OptionsBase _options;

        public DataStrategyWithOptions(DataStrategy wrappedStrategy, OptionsBase options)
        {
            _options = options;
            _wrappedStrategy = wrappedStrategy.Clone();
            _wrappedStrategy.GetAdapter().Options = options;
        }

        public override Adapter GetAdapter()
        {
            var adapter = _wrappedStrategy.GetAdapter();
            adapter.Options = _options;
            return adapter;
        }

        public SimpleTransaction BeginTransaction()
        {
            return SimpleTransaction.Begin(this);
        }

        public SimpleTransaction BeginTransaction(string name)
        {
            return SimpleTransaction.Begin(this, name);
        }

        public SimpleTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return SimpleTransaction.Begin(this, isolationLevel);
        }

        protected internal override bool ExecuteFunction(out object result, ExecuteFunctionCommand command)
        {
            return _wrappedStrategy.ExecuteFunction(out result, command);
        }

        protected internal override DataStrategy GetDatabase()
        {
            return _wrappedStrategy.GetDatabase();
        }

        internal override RunStrategy Run
        {
            get { return _wrappedStrategy.Run; }
        }

        protected internal override DataStrategy Clone()
        {
            return new DataStrategyWithOptions(_wrappedStrategy, _options);
        }

        public override dynamic ClearOptions()
        {
            _wrappedStrategy.GetAdapter().Options = null;
            return _wrappedStrategy;
        }
    }
}