using System.Collections.Generic;

namespace Simple.Data
{
    public class OptimizedDictionaryIndex<T>
    {
        private readonly Dictionary<T, int> _index;

        internal OptimizedDictionaryIndex(IDictionary<T,int> index)
        {
            _index = new Dictionary<T, int>(index);
        }

        public int this[T key]
        {
            get { return _index[key]; }
        }

        public IEnumerable<T> GetKeys()
        {
            return _index.Keys;
        }

        public bool ContainsKey(T key)
        {
            return _index.ContainsKey(key);
        }

        public bool TryGetIndex(T key, out int index)
        {
            return _index.TryGetValue(key, out index);
        }
    }
}
