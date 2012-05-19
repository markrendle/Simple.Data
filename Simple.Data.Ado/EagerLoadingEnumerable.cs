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
            var load = BuildLoadDictionary();

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
                    if (sub.Value.Single != null)
                    {
                        row[sub.Key] = sub.Value.Single;
                    }
                    else if (sub.Value.Collection != null)
                    {
                        row[sub.Key] = sub.Value.Collection.ToList();
                    }
                }

                yield return row;
            }
        }

        private Dictionary<IDictionary<string, object>, IDictionary<string, WithContainer>> BuildLoadDictionary()
        {
            var load =
                new Dictionary<IDictionary<string, object>, IDictionary<string, WithContainer>>(
                    new DictionaryEqualityComparer());

            foreach (var dict in _source)
            {
                IDictionary<string, WithContainer> withContainers;

                var main = new SubDictionary<string, object>(dict, s => !s.StartsWith("__with"));
                if (!load.TryGetValue(main, out withContainers))
                {
                    withContainers = new Dictionary<string, WithContainer>();
                    load.Add(main, withContainers);
                    foreach (var tuple in ExtractSingleObjects(dict))
                    {
                        var withContainer = new WithContainer();
                        withContainer.SetSingle(tuple.Item2);
                        if (!ReferenceEquals(withContainer.Single, null))
                        {
                            withContainers.Add(tuple.Item1, withContainer);
                        }
                    }
                }

                foreach (var tuple in ExtractCollectionObjects(dict))
                {
                    if (!withContainers.ContainsKey(tuple.Item1))
                    {
                        withContainers.Add(tuple.Item1, new WithContainer());
                    }
                    withContainers[tuple.Item1].AddToCollection(tuple.Item2);
                }
            }
            return load;
        }

        private class WithContainer
        {
            public HashSet<IDictionary<string, object>> Collection { get; private set; }

            public IDictionary<string, object> Single { get; private set; }

            public void AddToCollection(IDictionary<string,object> row)
            {
                if (row.All(kvp => ReferenceEquals(null, kvp.Value))) return;
                if (Collection == null) Collection = new HashSet<IDictionary<string, object>>(new DictionaryEqualityComparer());
                Collection.Add(row);
            }

            public void SetSingle(IDictionary<string,object> row)
            {
                if (row.All(kvp => ReferenceEquals(null, kvp.Value))) return;
                Single = row;
            }
        }

        private IEnumerable<Tuple<string,Dictionary<string,object>>> ExtractSingleObjects(IDictionary<string,object> source)
        {
            var names =
                source.Keys.Where(k => k.StartsWith("__with1__")).Select(
                    k => k.Split(new[] {"__"}, StringSplitOptions.RemoveEmptyEntries)[1]).Distinct().ToList();

            return from name in names
                   let pattern = "__with1__" + name + "__"
                   select Tuple.Create(name, source.Where(kvp => kvp.Key.StartsWith(pattern))
                                                 .ToDictionary(kvp => kvp.Key.Replace(pattern, ""), kvp => kvp.Value, HomogenizedEqualityComparer.DefaultInstance));
        }
        private IEnumerable<Tuple<string,Dictionary<string,object>>> ExtractCollectionObjects(IDictionary<string,object> source)
        {
            var names =
                source.Keys.Where(k => k.StartsWith("__withn__")).Select(
                    k => k.Split(new[] {"__"}, StringSplitOptions.RemoveEmptyEntries)[1]).Distinct().ToList();

            return from name in names
                   let pattern = "__withn__" + name + "__"
                   select Tuple.Create(name, source.Where(kvp => kvp.Key.StartsWith(pattern))
                                                 .ToDictionary(kvp => kvp.Key.Replace(pattern, ""), kvp => kvp.Value, HomogenizedEqualityComparer.DefaultInstance));
        }
    }
}