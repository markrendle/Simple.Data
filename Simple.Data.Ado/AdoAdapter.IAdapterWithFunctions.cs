using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
	internal partial class AdoAdapter : IAdapterWithFunctions
	{
	    public bool IsValidFunction(string functionName)
	    {
	        throw new NotImplementedException();
	    }

	    public FunctionReturnType GetReturnType(string functionName)
	    {
	        throw new NotImplementedException();
	    }

	    public object ExecuteScalar(string functionName, IEnumerable<KeyValuePair<string, object>> parameters)
	    {
	        throw new NotImplementedException();
	    }

	    public IEnumerable<IEnumerable<KeyValuePair<string, object>>> ExecuteResultSet(string functionName, IEnumerable<KeyValuePair<string, object>> parameters)
	    {
	        throw new NotImplementedException();
	    }

	    public IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> ExecuteMultipleResultSets(string functionName, IEnumerable<KeyValuePair<string, object>> parameters)
	    {
	        throw new NotImplementedException();
	    }
	}
}
