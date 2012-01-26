namespace Simple.Data.Ado
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using QueryPolyfills;

    public class EagerLoadingEnumerable : IEnumerable<IDictionary<string,object>>
    {
        private readonly IEnumerable<IDictionary<string, object>> _source;
        private readonly object _listLock = new object();
        private IList<IDictionary<string, object>> _list;

        public EagerLoadingEnumerable(IEnumerable<IDictionary<string, object>> source)
        {
            _source = source;
        }

        public IEnumerator<IDictionary<string, object>> GetEnumerator()
        {
            lock (_listLock)
            {
                if (_list == null)
                {
                    _list = CreateObjectGraphs().ToList();
                }
            }

            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<IDictionary<string,object>> CreateObjectGraphs()
        {
            var load = new Dictionary<IDictionary<string,object>,IDictionary<string, HashSet<IDictionary<string, object>>>>(new DictionaryEqualityComparer());

            foreach (var dict in _source)
            {
                IDictionary<string, HashSet<IDictionary<string, object>>> children;

                var main = new SubDictionary<string, object>(dict, s => !s.StartsWith("__with__"));
                if (!load.TryGetValue(main, out children))
                {
                    children = new Dictionary<string, HashSet<IDictionary<string, object>>>();
                    load.Add(main, children);
                }

                foreach (var tuple in ExtractObjects(dict))
                {
                    if (children.ContainsKey(tuple.Item1))
                    {
                        children[tuple.Item1].Add(tuple.Item2);
                    }
                    else
                    {
                        children.Add(tuple.Item1, new HashSet<IDictionary<string, object>>(new DictionaryEqualityComparer()) {tuple.Item2});
                    }
                }
            }

            IDictionary<string,int> index = null;
            foreach (var kvp in load)
            {
                if (index == null)
                {
                    index = kvp.Key.Keys.Select((k, i) => new KeyValuePair<string, int>(k, i)).ToDictionary(HomogenizedEqualityComparer.DefaultInstance);
                }
                var row = new OptimizedDictionary<string, object>(index, kvp.Key.Values);
                foreach (var sub in kvp.Value)
                {
                    if (sub.Value.Count == 1)
                    {
                        kvp.Key[sub.Key] = sub.Value.Single();
                    }
                    else
                    {
                        kvp.Key[sub.Key] = sub.Value.ToList();
                    }
                }

                yield return kvp.Key;
            }
        }

        private IEnumerable<Tuple<string,Dictionary<string,object>>> ExtractObjects(IDictionary<string,object> source)
        {
            var names =
                source.Keys.Where(k => k.StartsWith("__with__")).Select(
                    k => k.Split(new[] {"__"}, StringSplitOptions.RemoveEmptyEntries)[1]).ToList();

            return from name in names
                   let pattern = "__with__" + name + "__"
                   select Tuple.Create(name, source.Where(kvp => kvp.Key.StartsWith(pattern))
                                                 .ToDictionary(kvp => kvp.Key.Replace(pattern, ""), kvp => kvp.Value));
        }
    }

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