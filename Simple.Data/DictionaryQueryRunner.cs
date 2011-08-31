using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClauseHandler = System.Func<Simple.Data.SimpleQueryClauseBase, System.Collections.Generic.IEnumerable<System.Collections.Generic.IDictionary<string, object>>, System.Collections.Generic.IEnumerable<System.Collections.Generic.IDictionary<string, object>>>;

namespace Simple.Data
{
    internal class DictionaryQueryRunner
    {
        private static readonly
            Dictionary<Type, ClauseHandler> ClauseHandlers =
                new Dictionary<Type, ClauseHandler>
                    {
                        { typeof(DistinctClause), (c,d) => d.Distinct(new DictionaryEqualityComparer())},
                        { typeof(SkipClause), (c,d) => d.Skip(((SkipClause)c).Count)},
                        { typeof(TakeClause), (c,d) => d.Take(((TakeClause)c).Count)},
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
                ClauseHandler handler;
                if (ClauseHandlers.TryGetValue(clause.GetType(), out handler))
                {
                    source = handler(clause, source);
                }
            }

            return source;
        }
    }

    public class DictionaryEqualityComparer : IEqualityComparer<IDictionary<string, object>>
    {
        public bool Equals(IDictionary<string, object> x, IDictionary<string, object> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
            if (x.Count != y.Count) return false;
            object yvalue;
            return x.Keys.All(key => y.TryGetValue(key, out yvalue) && Equals(x[key], yvalue));
        }

        public int GetHashCode(IDictionary<string, object> obj)
        {
            return obj.Aggregate(0,
                                 (acc, kvp) => (((acc * 397) ^ kvp.Key.GetHashCode()) * 397) ^ (kvp.Value ?? DBNull.Value).GetHashCode());
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
