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
        private readonly IDictionary<string, object> _arguments;

        public ExecuteFunctionCommand(IAdapterWithFunctions adapter, string functionName, IDictionary<string,object> arguments)
        {
            _adapter = adapter;
            _functionName = functionName;
            _arguments = arguments;
        }

        public bool Execute(out object result)
        {
            result = ToMultipleResultSets(_adapter.Execute(_functionName, _arguments));
            return true;
        }

        private SimpleResultSet ToMultipleResultSets(object source)
        {
            if (source == null) return SimpleResultSet.Empty;
            var resultSets = source as IEnumerable<IEnumerable<IDictionary<string, object>>>;
            if (resultSets == null) throw new InvalidOperationException("Adapter returned incorrect Type.");

            return ToMultipleDynamicEnumerables(resultSets);
        }

        private SimpleResultSet ToMultipleDynamicEnumerables(IEnumerable<IEnumerable<IDictionary<string, object>>> resultSets)
        {
            var result = new SimpleResultSet(resultSets.Select(resultSet => resultSet.Select(dict => new SimpleRecord(dict))));
            result.SetOutputValues(_arguments);
            return result;
        }

        private static SimpleResultSet ToResultSet(object source)
        {
            if (source == null) return SimpleResultSet.Empty;

            var dicts = source as IEnumerable<IDictionary<string, object>>;
            if (dicts == null) throw new InvalidOperationException("Adapter returned incorrect Type.");

            return new SimpleResultSet(dicts.Select(dict => new SimpleRecord(dict)));
        }
    }
}
