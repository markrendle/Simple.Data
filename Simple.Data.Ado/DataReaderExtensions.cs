using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Simple.Data.Ado
{
    internal static class DataReaderExtensions
    {
        public static IEnumerable<IDictionary<string, object>> ToDictionaries(this IDataReader reader)
        {
            var list = new List<IDictionary<string, object>>();
            using (reader)
            {
                while (reader.Read())
                {
                    list.Add(reader.ToDictionary());
                }
            }

            return list.AsEnumerable();
        }
    }
}
