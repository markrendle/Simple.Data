using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResultSet = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>>;

namespace Simple.Data.Ado
{
	internal partial class AdoAdapter : IAdapterWithFunctions
	{
	    private readonly ConcurrentDictionary<string, ProcedureExecutor> _executors = new ConcurrentDictionary<string, ProcedureExecutor>();

	    public bool IsValidFunction(string functionName)
	    {
	        return _schema.FindProcedure(functionName) != null;
	    }

	    public IEnumerable<ResultSet> Execute(string functionName, IDictionary<string, object> parameters)
	    {
	        var executor = _executors.GetOrAdd(functionName, f => new ProcedureExecutor(this, ObjectName.Parse(f)));
	        return executor.Execute(parameters);
	    }
	}
}
