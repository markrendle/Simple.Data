using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    static class EnumerableExtensions
    {
        public static T BestMatch<T>(this IEnumerable<T> source, params Func<T,bool>[] predicates)
        {
            foreach (var predicate in predicates)
            {
                try
                {
                    return source.Single(predicate);
                }
                catch (InvalidOperationException)
                {
                }
            }

            throw new InvalidOperationException();
        }

        public static T BestMatchOrDefault<T>(this IEnumerable<T> source, params Func<T, bool>[] predicates)
        {
            foreach (var predicate in predicates)
            {
                try
                {
                    return source.Single(predicate);
                }
                catch (InvalidOperationException)
                {
                }
            }

            return default(T);
        }

        public static Dictionary<TKey,TValue> ToDictionary<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> source)
        {
            return source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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
    }
}
