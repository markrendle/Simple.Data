namespace Simple.Data.QueryPolyfills
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    internal class DictionaryQueryRunner
    {
        private static readonly
            Dictionary<Type, Func<SimpleQueryClauseBase, IEnumerable<IDictionary<string, object>>, IEnumerable<IDictionary<string, object>>>> ClauseHandlers =
                new Dictionary<Type, Func<SimpleQueryClauseBase, IEnumerable<IDictionary<string, object>>, IEnumerable<IDictionary<string, object>>>>
                    {
                        { typeof(DistinctClause), (c,d) => d.Distinct(new DictionaryEqualityComparer()) },
                        { typeof(SkipClause), (c,d) => d.Skip(((SkipClause)c).Count) },
                        { typeof(TakeClause), (c,d) => d.Take(((TakeClause)c).Count) },
                        { typeof(SelectClause), (c,d) => new SelectClauseHandler((SelectClause)c).Run(d) },
                    };

        private readonly IEnumerable<IDictionary<string, object>> _source;
        private readonly IList<SimpleQueryClauseBase> _clauses;
        private readonly WithCountClause _withCountClause;

        public DictionaryQueryRunner(IEnumerable<IDictionary<string, object>> source, IEnumerable<SimpleQueryClauseBase> clauses)
        {
            _source = source;
            _clauses = clauses.ToList();
            _withCountClause = _clauses.OfType<WithCountClause>().FirstOrDefault();
            if (_withCountClause != null) _clauses.Remove(_withCountClause);
        }

        public DictionaryQueryRunner(IEnumerable<IDictionary<string, object>> source, params SimpleQueryClauseBase[] clauses)
            : this(source, clauses.AsEnumerable())
        {
        }

        public IEnumerable<IDictionary<string, object>> Run()
        {
            IEnumerable<IDictionary<string, object>> source;
            if (_withCountClause != null)
            {
                source = _source.ToList();
                _withCountClause.SetCount(source.Count());
            }
            else
            {
                source = _source;
            }

            foreach (var clause in _clauses)
            {
                Func<SimpleQueryClauseBase, IEnumerable<IDictionary<string, object>>, IEnumerable<IDictionary<string, object>>> handler;
                if (ClauseHandlers.TryGetValue(clause.GetType(), out handler))
                {
                    source = handler(clause, source);
                }
            }

            return source;
        }
    }

    class SelectClauseHandler
    {
        private readonly IList<SimpleReference> _references;
        private readonly IList<ObjectReference> _objectReferences;
        private Func<int, IDictionary<string, object>> _creator;

        public SelectClauseHandler(SelectClause clause)
        {
            _references = clause.Columns.ToList();
            _objectReferences = _references.Select(r => r as ObjectReference).ToList();
        }

        public IEnumerable<IDictionary<string,object>> Run(IEnumerable<IDictionary<string,object>> source)
        {
            return source.Select(Run);
        }

        private IDictionary<string,object> Run(IDictionary<string,object> source)
        {
            if (_creator == null) _creator = CreateCreator(source);
            var target = _creator(_references.Count);
            for (int i = 0; i < _objectReferences.Count; i++)
            {
                if (!ReferenceEquals(_objectReferences[i], null))
                {
                    var name = _objectReferences[i].GetName();
                    target[name] = source[name];
                }
            }
            return target;
        }

        private Func<int, IDictionary<string, object>> CreateCreator(IDictionary<string,object> source)
        {
            var dictionary = source as Dictionary<string, object>;
            if (dictionary != null) return cap => new Dictionary<string, object>(cap, dictionary.Comparer);
            var sortedDictionary = source as SortedDictionary<string, object>;
            if (sortedDictionary != null) return cap => new SortedDictionary<string, object>(sortedDictionary.Comparer);
            var concurrentDictionary = source as ConcurrentDictionary<string, object>;
            if (concurrentDictionary != null) return cap => new ConcurrentDictionary<string, object>();

            var type = source.GetType();
            return cap => (IDictionary<string, object>) Activator.CreateInstance(type);
        }
    }
}
