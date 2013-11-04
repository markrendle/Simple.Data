using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    static class TupleExtensions
    {
        public static IEnumerable<TResult> TupleSelect<T1,T2,TResult>(this IEnumerable<Tuple<T1,T2>> source, Func<T1,T2,TResult> selector)
        {
            return source.Select(tuple => selector(tuple.Item1, tuple.Item2));
        }
    }
}
