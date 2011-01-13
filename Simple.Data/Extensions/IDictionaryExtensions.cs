using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Extensions
{
    internal static class DictionaryExtensions
    {
        public static SimpleRecord ToDynamicRecord(this IDictionary<string, object> dictionary)
        {
            return dictionary == null ? null : new SimpleRecord(dictionary);
        }

        public static SimpleRecord ToDynamicRecord(this IDictionary<string, object> dictionary, string tableName)
        {
            return dictionary == null ? null : new SimpleRecord(dictionary, tableName);
        }

        public static SimpleRecord ToDynamicRecord(this IDictionary<string, object> dictionary, string tableName, DataStrategy dataStrategy)
        {
            return dictionary == null ? null : new SimpleRecord(dictionary, tableName, dataStrategy);
        }
    }
}
