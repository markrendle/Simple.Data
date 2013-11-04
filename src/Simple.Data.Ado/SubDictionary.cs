namespace Simple.Data.Ado
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class SubDictionary<TKey,TValue> : IDictionary<TKey,TValue>
    {
        private readonly IDictionary<TKey, TValue> _super;
        private readonly Func<TKey, bool> _keyFilter;

        private IEnumerable<KeyValuePair<TKey, TValue>> Filter()
        {
            return _super.Where(kvp => _keyFilter(kvp.Key));
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Filter().GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _super.Add(item);
        }

        public void Clear()
        {
            _super.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _super.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Filter().ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _super.Remove(item);
        }

        public int Count
        {
            get { return Filter().Count(); }
        }

        public bool IsReadOnly
        {
            get { return _super.IsReadOnly; }
        }

        public bool ContainsKey(TKey key)
        {
            return _keyFilter(key) && _super.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            _super.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            return _super.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _super.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get { return _super[key]; }
            set { _super[key] = value; }
        }

        public ICollection<TKey> Keys
        {
            get { return _super.Keys.Where(_keyFilter).ToList().AsReadOnly(); }
        }

        public ICollection<TValue> Values
        {
            get { return Filter().Select(kvp => kvp.Value).ToList().AsReadOnly(); }
        }

        public SubDictionary(IDictionary<TKey, TValue> super, Func<TKey,bool> keyFilter)
        {
            _super = super;
            _keyFilter = keyFilter;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}