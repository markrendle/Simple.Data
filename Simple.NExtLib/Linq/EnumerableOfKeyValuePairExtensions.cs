using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Linq
{
    public static class EnumerableOfKeyValuePairExtensions
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            Dictionary<TKey, TValue> dictionary;

            if ((dictionary = source as Dictionary<TKey, TValue>) == null)
            {
                dictionary = source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            return dictionary;
        }

        public static IDictionary<TKey, TValue> ToIDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            Dictionary<TKey, TValue> dictionary;

            if ((dictionary = source as Dictionary<TKey, TValue>) == null)
            {
                dictionary = source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            return dictionary;
        }
    }
}
