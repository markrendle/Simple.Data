using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.Extensions
{
    public static class EnumerableExtensions
    {
        public static IDictionary<TKey,TValue> ToDictionary<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> source)
        {
            var dict = source as IDictionary<TKey, TValue>;
            if (dict != null) return new Dictionary<TKey, TValue>(dict);
            return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static IDictionary<TKey,TValue> ToDictionary<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> source, IEqualityComparer<TKey> equalityComparer)
        {
            var dict = source as IDictionary<TKey, TValue>;
            if (dict != null) return new Dictionary<TKey, TValue>(dict);
            return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, equalityComparer);
        }

        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source)
        {
            var buffer = default(T);
            var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                buffer = enumerator.Current;
            }
            while (enumerator.MoveNext())
            {
                yield return buffer;
                buffer = enumerator.Current;
            }
        }

        public static IEnumerable<Tuple<T,T>> ToTuplePairs<T>(this IEnumerable<T> source)
        {
            var buffer = default(T);
            var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                buffer = enumerator.Current;
            }
            while (enumerator.MoveNext())
            {
                yield return Tuple.Create(buffer, enumerator.Current);
                buffer = enumerator.Current;
            }
        }

        public static IEnumerable<T> ExtendInfinite<T>(this IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            while (true)
            {
                yield return default(T);
            }
        }

        public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, T toReplace, T replaceWith)
        {
            return source.Select(item => Equals(item, toReplace) ? replaceWith : item);
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
        {
            return source.Concat(Return(item));
        }

        public static IEnumerable<T> Return<T>(T item)
        {
            yield return item;
        }
    }
}
