namespace Simple.Data.QueryPolyfills
{
    using System;
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
                        { typeof(WhereClause), (c,d) => new WhereClauseHandler((WhereClause)c).Run(d) }
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
}
