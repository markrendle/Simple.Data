using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.Ado
{
    public static class OptimizedDictionary
    {
        public static OptimizedDictionaryIndex<T> CreateIndex<T>(IEnumerable<T> keys)
        {
            var index = keys.Select((key, i) => new KeyValuePair<T, int>(key, i)).ToDictionary(kvp => kvp.Key,
                                                                                            kvp => kvp.Value);
            return new OptimizedDictionaryIndex<T>(index);
        }

        public static OptimizedDictionary<TKey,TValue> Create<TKey,TValue>(IDictionary<TKey,int> index, IEnumerable<TValue> values)
        {
            return new OptimizedDictionary<TKey, TValue>(index, values);
        }
    }
}
