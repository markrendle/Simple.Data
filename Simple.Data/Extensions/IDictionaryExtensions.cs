using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Extensions
{
    internal static class DictionaryExtensions
    {
        public static DynamicRecord ToDynamicRecord(this IDictionary<string, object> dictionary)
        {
            return dictionary == null ? null : new DynamicRecord(dictionary);
        }

        public static DynamicRecord ToDynamicRecord(this IDictionary<string, object> dictionary, string tableName)
        {
            return dictionary == null ? null : new DynamicRecord(dictionary, tableName);
        }

        public static DynamicRecord ToDynamicRecord(this IDictionary<string, object> dictionary, string tableName, Database database)
        {
            return dictionary == null ? null : new DynamicRecord(dictionary, tableName, database);
        }
    }
}
