using System.Collections.Generic;
using System.Linq;

namespace Shitty.Data.Extensions
{
    internal static class ResultSetExtensions
    {
        public static SimpleResultSet ResultSetFromModifiedRowCount(this int count)
        {
            var simpleResultSet = new SimpleResultSet(Enumerable.Empty<SimpleRecord>());
            simpleResultSet.SetOutputValues(new Dictionary<string, object> { { "__ReturnValue", count } });
            return simpleResultSet;
        }
    }
}