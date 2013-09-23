using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResultSet = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>>;

namespace Simple.Data.Ado
{
    public partial class AdoAdapter
	{
	    private readonly ConcurrentDictionary<string, IProcedureExecutor> _executors = new ConcurrentDictionary<string, IProcedureExecutor>();

	    public bool IsValidFunction(string functionName)
	    {
	        return _connectionProvider.SupportsStoredProcedures && _schema.IsProcedure(functionName);
	    }

	    public IEnumerable<ResultSet> Execute(string functionName, IDictionary<string, object> parameters)
	    {
	        var executor = _executors.GetOrAdd(functionName, f => _connectionProvider.GetProcedureExecutor(this, _schema.BuildObjectName(f)));
	        return executor.Execute(parameters);
	    }

        public IEnumerable<ResultSet> Execute(string functionName, IDictionary<string, object> parameters, IAdapterTransaction transaction)
        {
            var executor = _executors.GetOrAdd(functionName, f => _connectionProvider.GetProcedureExecutor(this, _schema.BuildObjectName(f)));
            return executor.Execute(parameters, ((AdoAdapterTransaction)transaction).DbTransaction);
        }
	}
}
