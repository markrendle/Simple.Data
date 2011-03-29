using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.Ado
{
    public static class OptimizedDictionary
    {
        public static OptimizedDictionary<TKey,TValue> Create<TKey,TValue>(IDictionary<TKey,int> index, IEnumerable<TValue> values)
        {
            return new OptimizedDictionary<TKey, TValue>(index, values);
        }
    }
}
