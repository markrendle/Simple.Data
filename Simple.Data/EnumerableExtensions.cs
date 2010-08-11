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
    }
}
