using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    class WeakCache<T>
    {
        private readonly Action _cleanup;
        private readonly List<Entry> _cache = new List<Entry>();

        public WeakCache()
        {
            _cleanup = CleanUp;
        }

        public void Add(object key, T value)
        {
            _cache.Add(new Entry(new WeakReference(key), value));
            _cleanup.BeginInvoke(_cleanup.EndInvoke, null);
        }

        public T Get(object key)
        {
            return _cache.Single(e => key.Equals(e.Reference.Target)).Value;
        }

        public bool Contains(object key)
        {
            return _cache.Any(e => key.Equals(e.Reference.Target));
        }

        private void CleanUp()
        {
            var deadPool = _cache.Where(e => !e.Reference.IsAlive).ToList();
            deadPool.ForEach(e => _cache.Remove(e));
        }

        private class Entry
        {
            private readonly WeakReference _reference;
            private readonly T _value;

            public Entry(WeakReference reference, T value)
            {
                _reference = reference;
                _value = value;
            }

            public T Value
            {
                get { return _value; }
            }

            public WeakReference Reference
            {
                get { return _reference; }
            }
        }
    }
}
