using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.NExtLib.Linq
{
    public static class TupleEnumerableExtensions
    {
        public static bool Any<T1, T2>(this IEnumerable<Tuple<T1, T2>> source,
            Func<T1, T2, bool> predicate)
        {
            return source.Any(tuple => predicate(tuple.Item1, tuple.Item2));
        }

        public static bool All<T1, T2>(this IEnumerable<Tuple<T1, T2>> source,
            Func<T1, T2, bool> predicate)
        {
            return source.All(tuple => predicate(tuple.Item1, tuple.Item2));
        }

        public static int Count<T1, T2>(this IEnumerable<Tuple<T1, T2>> source,
            Func<T1, T2, bool> predicate)
        {
            return source.Count(tuple => predicate(tuple.Item1, tuple.Item2));
        }

        public static long LongCount<T1, T2>(this IEnumerable<Tuple<T1, T2>> source,
            Func<T1, T2, bool> predicate)
        {
            return source.LongCount(tuple => predicate(tuple.Item1, tuple.Item2));
        }

        public static Tuple<T1, T2> First<T1, T2>(this IEnumerable<Tuple<T1, T2>> source,
            Func<T1, T2, bool> predicate)
        {
            return source.First(tuple => predicate(tuple.Item1, tuple.Item2));
        }

        public static Tuple<T1, T2> FirstOrDefault<T1, T2>(this IEnumerable<Tuple<T1, T2>> source,
            Func<T1, T2, bool> predicate)
        {
            return source.FirstOrDefault(tuple => predicate(tuple.Item1, tuple.Item2));
        }

        public static Tuple<T1, T2> Last<T1, T2>(this IEnumerable<Tuple<T1, T2>> source,
            Func<T1, T2, bool> predicate)
        {
            return source.Last(tuple => predicate(tuple.Item1, tuple.Item2));
        }

        public static Tuple<T1, T2> LastOrDefault<T1, T2>(this IEnumerable<Tuple<T1, T2>> source,
            Func<T1, T2, bool> predicate)
        {
            return source.LastOrDefault(tuple => predicate(tuple.Item1, tuple.Item2));
        }

        public static IEnumerable<TResult> Select<T1, T2, TResult>(this IEnumerable<Tuple<T1, T2>> source,
            Func<T1, T2, TResult> selector)
        {
            return source.Select(tuple => selector(tuple.Item1, tuple.Item2));
        }

        public static IEnumerable<TResult> SelectMany<T1, T2, TResult>(this IEnumerable<Tuple<T1, T2>> source,
        Func<T1, T2, IEnumerable<TResult>> selector)
        {
            return source.SelectMany(tuple => selector(tuple.Item1, tuple.Item2));
        }

        public static Tuple<T1, T2> Single<T1, T2>(this IEnumerable<Tuple<T1, T2>> source,
        Func<T1, T2, bool> predicate)
        {
            return source.Single(tuple => predicate(tuple.Item1, tuple.Item2));
        }

        public static Tuple<T1, T2> SingleOrDefault<T1, T2>(this IEnumerable<Tuple<T1, T2>> source,
            Func<T1, T2, bool> predicate)
        {
            return source.SingleOrDefault(tuple => predicate(tuple.Item1, tuple.Item2));
        }
    }
}
