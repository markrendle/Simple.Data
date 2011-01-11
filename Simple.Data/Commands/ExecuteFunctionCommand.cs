using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Simple.Data.Commands
{
    class ExecuteFunctionCommand
    {
        private readonly IAdapterWithFunctions _adapter;
        private readonly string _functionName;
        private readonly IEnumerable<string> _argumentNames;
        private readonly IEnumerable<object> _arguments;

        public ExecuteFunctionCommand(IAdapterWithFunctions adapter)
        {
            _adapter = adapter;
        }

        public ExecuteFunctionCommand(IAdapterWithFunctions adapter, string functionName, IEnumerable<string> argumentNames, IEnumerable<object> arguments)
        {
            _adapter = adapter;
            _functionName = functionName;
            _argumentNames = argumentNames;
            _arguments = arguments;
        }

        public bool Execute(out object result)
        {
            var resultType = _adapter.GetReturnType(_functionName);
            var func = GetFunc(resultType);
            result = func(_functionName, MakeArguments());
            return true;
        }

        private Func<string, IEnumerable<KeyValuePair<string, object>>, object> GetFunc(FunctionReturnType resultType)
        {
            Func<string, IEnumerable<KeyValuePair<string, object>>, object> func = null;

            switch (resultType)
            {
                case FunctionReturnType.ResultSet:
                    func = _adapter.ExecuteResultSet;
                    break;
                case FunctionReturnType.MultipleResultSets:
                    func = _adapter.ExecuteMultipleResultSets;
                    break;
                default:
                    func = _adapter.ExecuteScalar;
                    break;
            }
            return func;
        }

        private IEnumerable<KeyValuePair<string,object>> MakeArguments()
        {
            var argumentNamesWithDefaults =
                _argumentNames.Select((s, i) => string.IsNullOrWhiteSpace(s) ? i.ToString() : s);
            return _arguments.Zip(argumentNamesWithDefaults,
                (v, k) => new KeyValuePair<string, object>(k, v));
        }
    }
}
