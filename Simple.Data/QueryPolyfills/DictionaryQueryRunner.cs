namespace Simple.Data.QueryPolyfills
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class DictionaryQueryRunner
    {
        const string AutoColumnPrefix = "___having___";
        private static readonly
            Dictionary<Type, Func<SimpleQueryClauseBase, IEnumerable<IDictionary<string, object>>, IEnumerable<IDictionary<string, object>>>> ClauseHandlers =
                new Dictionary<Type, Func<SimpleQueryClauseBase, IEnumerable<IDictionary<string, object>>, IEnumerable<IDictionary<string, object>>>>
                    {
                        { typeof(DistinctClause), (c,d) => d.Distinct(new DictionaryEqualityComparer()) },
                        { typeof(SkipClause), (c,d) => d.Skip(((SkipClause)c).Count) },
                        { typeof(TakeClause), (c,d) => d.Take(((TakeClause)c).Count) }
                    };

        private readonly string _mainTableName;
        private readonly IEnumerable<IDictionary<string, object>> _source;
        private readonly IList<SimpleQueryClauseBase> _clauses;
        private readonly WithCountClause _withCountClause;

        public DictionaryQueryRunner(string mainTableName, IEnumerable<IDictionary<string, object>> source, IEnumerable<SimpleQueryClauseBase> clauses)
        {
            _mainTableName = mainTableName;
            _source = source;
            _clauses = clauses.ToList();
            _withCountClause = _clauses.OfType<WithCountClause>().FirstOrDefault();
            if (_withCountClause != null) _clauses.Remove(_withCountClause);
        }

        public DictionaryQueryRunner(string mainTableName, IEnumerable<IDictionary<string, object>> source, params SimpleQueryClauseBase[] clauses)
            : this(mainTableName, source, clauses.AsEnumerable())
        {
        }

        public IEnumerable<IDictionary<string, object>> Run()
        {
            var source = RunWhereClauses(_source);

            if (_withCountClause != null)
            {
                var list = source.ToList();
                _withCountClause.SetCount(list.Count);
                source = list;
            }

            source = RunOrderByClauses(source);

            foreach (var clause in _clauses)
            {
                Func<SimpleQueryClauseBase, IEnumerable<IDictionary<string, object>>, IEnumerable<IDictionary<string, object>>> handler;
                if (ClauseHandlers.TryGetValue(clause.GetType(), out handler))
                {
                    source = handler(clause, source);
                }
            }
            
            source = RunHavingClauses(source);
            source = RunSelectClauses(source);
            return source;
        }

        private IEnumerable<IDictionary<string, object>> RunOrderByClauses(IEnumerable<IDictionary<string, object>> source)
        {
            var orderByClauses = _clauses.OfType<OrderByClause>().Reverse();
            foreach (var orderByClause in orderByClauses)
            {
                source = new OrderByClauseHandler(orderByClause).Run(source);
            }
            return source;
        }

        private IEnumerable<IDictionary<string, object>> RunWhereClauses(IEnumerable<IDictionary<string, object>> source)
        {
            foreach (var whereClause in _clauses.OfType<WhereClause>())
            {
                source = new WhereClauseHandler(_mainTableName, whereClause).Run(source);
            }
            return source;
        }

        private IEnumerable<IDictionary<string, object>> RunSelectClauses(IEnumerable<IDictionary<string, object>> source)
        {
            foreach (var selectClause in _clauses.OfType<SelectClause>())
            {
                source = new SelectClauseHandler(selectClause).Run(source);
            }
            return source;
        }

        private IEnumerable<IDictionary<string, object>> RunHavingClauses(IEnumerable<IDictionary<string, object>> source)
        {
            var havingClauses = _clauses.OfType<HavingClause>().ToList();
            if (havingClauses.Count == 0) return source;

            var selectClause = _clauses.OfType<SelectClause>().FirstOrDefault();

            List<SimpleReference> selectReferences;

            if (selectClause != null)
            {
                selectReferences = selectClause.Columns.ToList();
            }
            else
            {
                selectReferences = new List<SimpleReference> { new AllColumnsSpecialReference() };
            }

            foreach (var clause in havingClauses)
            {
                var criteria = HavingToWhere(clause.Criteria, selectReferences);
                source = new SelectClauseHandler(new SelectClause(selectReferences)).Run(source).ToList();
                source = new WhereClauseHandler(_mainTableName, new WhereClause(criteria)).Run(source);
                source = source.Select(d => d.Where(kvp => !kvp.Key.StartsWith(AutoColumnPrefix)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            }

            return source;
        }

        private SimpleExpression HavingToWhere(SimpleExpression criteria, List<SimpleReference> selectReferences)
        {
            if (criteria.LeftOperand is SimpleExpression)
            {
                return new SimpleExpression(HavingToWhere((SimpleExpression) criteria.LeftOperand, selectReferences),
                                            HavingToWhere((SimpleExpression) criteria.RightOperand, selectReferences),
                                            criteria.Type);
            }

            object leftOperand = ReplaceFunctionOperand(criteria.LeftOperand, selectReferences);
            object rightOperand = ReplaceFunctionOperand(criteria.RightOperand, selectReferences);

            return new SimpleExpression(leftOperand, rightOperand, criteria.Type);
        }

        private static object ReplaceFunctionOperand(object operand, List<SimpleReference> selectReferences)
        {
            var leftFunction = operand as FunctionReference;
            if (!leftFunction.IsNull())
            {
                var alias = AutoColumnPrefix + Guid.NewGuid().ToString("N");
                selectReferences.Add(leftFunction.As(alias));
                return new ObjectReference(alias);
            }
            return operand;
        }

        private static bool AreEquivalentReferences(FunctionReference reference1, FunctionReference reference2)
        {
            if (reference1.IsNull()) return reference2.IsNull();
            if (reference2.IsNull()) return false;
            return reference1.Name == reference2.Name && reference1.Argument == reference2.Argument;
        }
    }

    internal class GenericEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        public GenericEqualityComparer(Func<T, T, bool> @equals) : this(@equals, _ => 1)
        {
        }

        public GenericEqualityComparer(Func<T, T, bool> @equals, Func<T, int> getHashCode)
        {
            _equals = @equals;
            _getHashCode = getHashCode;
        }

        public bool Equals(T x, T y)
        {
            return _equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _getHashCode(obj);
        }
    }
}
