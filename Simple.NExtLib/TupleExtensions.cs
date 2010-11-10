using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.NExtLib
{
    public static class TupleExtensions
    {
        public static void Run<T1, T2>(this Tuple<T1, T2> tuple, Action<T1, T2> action)
        {
            action(tuple.Item1, tuple.Item2);
        }

        public static void IfGood<T>(this Tuple<bool, T> tuple, Action<T> action)
        {
            if (tuple.Item1)
            {
                action(tuple.Item2);
            }
        }
    }
}
