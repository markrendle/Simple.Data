using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResultSet = System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, object>>>;

namespace Simple.Data
{
    /// <summary>
    /// Represents an Adapter which supports functions; for example, the Ado adapter supports this with Stored Procedures.
    /// </summary>
    /// <remarks>It may be possible to add functions to access data-store specific functionality (CreateIndex?) with this interface.</remarks>
    public interface IAdapterWithFunctions
    {
        bool IsValidFunction(string functionName);
        IEnumerable<ResultSet> Execute(string functionName, IDictionary<string, object> parameters);
    }
}
