using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResultSet = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>>;

namespace Simple.Data
{
    public interface IAdapterWithFunctions
    {
        bool IsValidFunction(string functionName);
        FunctionReturnType GetReturnType(string functionName);
        object ExecuteScalar(string functionName, IEnumerable<KeyValuePair<string, object>> parameters);
        ResultSet ExecuteResultSet(string functionName, IEnumerable<KeyValuePair<string, object>> parameters);
        IEnumerable<ResultSet> ExecuteMultipleResultSets(string functionName, IEnumerable<KeyValuePair<string, object>> parameters);
    }
}
