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
        IEnumerable<ResultSet> Execute(string functionName, IEnumerable<KeyValuePair<string, object>> parameters);
    }
}
