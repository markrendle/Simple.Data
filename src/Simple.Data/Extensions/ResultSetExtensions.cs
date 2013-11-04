using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.Extensions
{
    internal static class ResultSetExtensions
    {
        public static SimpleResultSet ResultSetFromModifiedRowCount(this int count)
        {
            var simpleResultSet = new SimpleResultSet(Enumerable.Empty<SimpleRecord>());
            IDictionary<string, object> outputValues = new Dictionary<string, object> { { "__ReturnValue", count } };
            simpleResultSet.SetOutputValues(outputValues);
            return simpleResultSet;
        }
    }
}