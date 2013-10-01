using System;
using System.Collections.Generic;

namespace Simple.Data
{
    using System.Collections.ObjectModel;

    public static class EnumerableEx
    {
        public static Func<Maybe<T>> ToIterator<T>(this IEnumerable<T> source)
        {
            var enumerator = source.GetEnumerator();
            return
                () => enumerator.MoveNext() ? enumerator.Current : Maybe<T>.None;
        }

        public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return ToReadOnlyDictionary(source, EqualityComparer<TKey>.Default);
        }

        public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> comparer)
        {
            ICollection<KeyValuePair<TKey, TValue>> dict = new Dictionary<TKey, TValue>(comparer);
            foreach (var pair in source)
            {
                dict.Add(pair);
            }
            return new ReadOnlyDictionary<TKey, TValue>((IDictionary<TKey, TValue>) dict);
        }

        public static IEnumerable<T> Once<T>(T item)
        {
            yield return item;
        }

        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> sourcePairs)
        {
            ICollection<KeyValuePair<TKey, TValue>> dict = new Dictionary<TKey, TValue>();
            foreach (var pair in sourcePairs)
            {
                dict.Add(pair);
            }
            return (IDictionary<TKey, TValue>) dict;
        }
    }
}
