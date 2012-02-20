using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    public class ExecuteFunctionCommand
    {
        private readonly Database _database;
        private readonly IAdapterWithFunctions _adapter;
        private readonly string _functionName;
        private readonly IDictionary<string, object> _arguments;

        public ExecuteFunctionCommand(Database database, IAdapterWithFunctions adapter, string functionName, IDictionary<string,object> arguments)
        {
            _database = database;
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

        public bool Execute(out object result, IAdapterTransaction adapterTransaction)
        {
            result = ToMultipleResultSets(_adapter.Execute(_functionName, _arguments, adapterTransaction));
            return true;
        }
    }
}
