using System.Collections.Generic;

namespace Shitty.Data.Ado
{
    using System.Linq;
    using Extensions;

    public class OptimizedDictionaryIndex<T>
    {
        private readonly Dictionary<T, int> _index;

        internal OptimizedDictionaryIndex(IDictionary<T,int> index)
        {
            _index = new Dictionary<T, int>(index);
        }

        protected OptimizedDictionaryIndex(IEnumerable<T> index)
        {
            _index = new Dictionary<T, int>();
            int i = 0;
            foreach (var key in index)
            {
                _index[key] = i++;
            }
        }

        public virtual int this[T key]
        {
            get { return _index[key]; }
        }

        public IEnumerable<T> GetKeys()
        {
            return _index.Keys;
        }

        public virtual bool ContainsKey(T key)
        {
            return _index.ContainsKey(key);
        }

        public virtual bool TryGetIndex(T key, out int index)
        {
            return _index.TryGetValue(key, out index);
        }
    }
}
