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

        public static IEnumerable<object> ToDynamicList(this IDataReader reader)
        {
            var list = new List<object>();
            using (reader)
            {
                while (reader.Read())
                {
                    list.Add(reader.ToDynamicRecord());
                }
            }

            return list.AsEnumerable();
        }

        public static IEnumerable<object> ToDynamicList(this IDataReader reader, Database database, string tableName)
        {
            var list = new List<object>();
            using (reader)
            {
                while (reader.Read())
                {
                    list.Add(reader.ToDynamicRecord(tableName, database));
                }
            }

            return list.AsEnumerable();
        }
    }
}
