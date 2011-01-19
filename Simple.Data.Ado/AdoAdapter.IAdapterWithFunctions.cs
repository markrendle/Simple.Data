using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResultSet = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>>;

namespace Simple.Data.Ado
{
	internal partial class AdoAdapter : IAdapterWithFunctions
	{
	    public bool IsValidFunction(string functionName)
	    {
	        return _schema.FindProcedure(functionName) != null;
	    }

	    public IEnumerable<ResultSet> Execute(string functionName, IEnumerable<KeyValuePair<string, object>> parameters)
	    {
	        var executor = new ProcedureExecutor(this, ObjectName.Parse(functionName));
	        return executor.Execute(parameters);
	    }
	}
}
