using Simple.Data.Commands;

namespace Simple.Data
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

        protected internal override bool ExecuteFunction(out object result, ExecuteFunctionCommand command)
        {
            return _wrappedStrategy.ExecuteFunction(out result, command);
        }

        protected internal override Database GetDatabase()
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