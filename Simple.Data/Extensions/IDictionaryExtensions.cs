using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Extensions
{
    using System.Collections.ObjectModel;

    internal static class DictionaryExtensions
    {
        public static SimpleRecord ToDynamicRecord(this IDictionary<string, object> dictionary, string tableName, DataStrategy dataStrategy)
        {
            return dictionary == null ? null : new SimpleRecord(dictionary, tableName, dataStrategy);
        }

        public static IReadOnlyDictionary<K, V> ToReadOnly<K, V>(this IDictionary<K, V> dictionary)
        {
            return new ReadOnlyDictionary<K, V>(dictionary);
        }
    }
}
