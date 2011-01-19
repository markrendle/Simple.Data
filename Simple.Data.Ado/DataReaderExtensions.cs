using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Simple.Data.Ado
{
    internal static class DataReaderExtensions
    {
        public static IEnumerable<IDictionary<string, object>> ToDictionaries(this IDataReader reader)
        {
            using (reader)
            {
                return ToDictionariesImpl(reader).ToArray().AsEnumerable();
            }
        }

        public static IEnumerable<IEnumerable<IDictionary<string, object>>> ToMultipleDictionaries(this IDataReader reader)
        {
            using (reader)
            {
                return ToMultipleDictionariesImpl(reader).ToArray().AsEnumerable();
            }
        }

        private static IEnumerable<IEnumerable<IDictionary<string,object>>> ToMultipleDictionariesImpl(IDataReader reader)
        {
            do
            {
                yield return ToDictionariesImpl(reader).ToArray().AsEnumerable();
            } while (reader.NextResult());
            
        }

        private static IEnumerable<IDictionary<string,object>> ToDictionariesImpl(IDataReader reader)
        {
            while (reader.Read())
            {
                yield return reader.ToDictionary();
            }
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
