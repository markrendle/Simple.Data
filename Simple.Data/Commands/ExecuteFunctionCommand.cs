using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    class ExecuteFunctionCommand
    {
        private readonly IAdapterWithFunctions _adapter;
        private readonly string _functionName;
        private readonly FunctionReturnType _returnType;
        private readonly IEnumerable<string> _argumentNames;
        private readonly IEnumerable<object> _arguments;

        public ExecuteFunctionCommand(IAdapterWithFunctions adapter, string functionName, IEnumerable<string> argumentNames, IEnumerable<object> arguments)
        {
            _adapter = adapter;
            _functionName = functionName;
            _argumentNames = argumentNames;
            _arguments = arguments;
            _returnType = _adapter.GetReturnType(_functionName);
        }

        public bool Execute(out object result)
        {
            var resultType = _adapter.GetReturnType(_functionName);
            var func = GetFunc(resultType);
            result = ConvertToSimpleTypes(func(_functionName, MakeArguments()));
            return true;
        }

        public FunctionReturnType ReturnType
        {
            get
            {
                return _returnType;
            }
        }

        private object ConvertToSimpleTypes(object source)
        {
            switch (_returnType)
            {
                case FunctionReturnType.ResultSet:
                    return ToResultSet(source);
                case FunctionReturnType.MultipleResultSets:
                    return ToMultipleResultSets(source);
                default:
                    return source;
            }
        }

        private static IEnumerable<SimpleResultSet> ToMultipleResultSets(object source)
        {
            if (source == null) return Enumerable.Empty<SimpleResultSet>();
            var resultSets = source as IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>;
            if (resultSets == null) throw new InvalidOperationException("Adapter returned incorrect Type.");

            return ToMultipleDynamicEnumerables(resultSets);
        }

        private static IEnumerable<SimpleResultSet> ToMultipleDynamicEnumerables(IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> resultSets)
        {
            return resultSets.Select(resultSet => new SimpleResultSet(resultSet.Select(dict => new SimpleRecord(dict))));
        }

        private static SimpleResultSet ToResultSet(object source)
        {
            if (source == null) return new SimpleResultSet(Enumerable.Empty<dynamic>());

            var dicts = source as IEnumerable<IEnumerable<KeyValuePair<string, object>>>;
            if (dicts == null) throw new InvalidOperationException("Adapter returned incorrect Type.");

            return new SimpleResultSet(dicts.Select(dict => new SimpleRecord(dict)));
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
            return _arguments.Reverse()
                .Zip(_argumentNames.Reverse().ExtendInfinite(), (v, k) => new KeyValuePair<string, object>(k, v))
                .Reverse()
                .Select((kvp, i) => kvp.Key == null ? new KeyValuePair<string, object>(i.ToString(), kvp.Value) : kvp);
        }
    }
}
