namespace Simple.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Extensions;
    using QueryPolyfills;

    public class SimpleQuery : DynamicObject, IEnumerable
    {
        private readonly Adapter _adapter;

        private readonly SimpleQueryClauseBase[] _clauses;
        private readonly string _tableName;
        private Func<IObservable<dynamic>> _asObservableImplementation;
        private DataStrategy _dataStrategy;
        private JoinClause _tempJoinWaitingForOn;

        public SimpleQuery(DataStrategy dataStrategy, string tableName)
        {
            _dataStrategy = dataStrategy;
            if (_dataStrategy != null)
                _adapter = _dataStrategy.GetAdapter();
            _tableName = tableName;
            _clauses = new SimpleQueryClauseBase[0];
        }

        public SimpleQuery(Adapter adapter, DataStrategy dataStrategy, string tableName)
        {
            _adapter = adapter;
            _dataStrategy = dataStrategy;
            _tableName = tableName;
            _clauses = new SimpleQueryClauseBase[0];
        }

        private SimpleQuery(SimpleQuery source,
                            SimpleQueryClauseBase[] clauses)
        {
            _adapter = source._adapter;
            _dataStrategy = source._dataStrategy;
            _tableName = source.TableName;
            _clauses = clauses;
        }

        private SimpleQuery(SimpleQuery source,
                            string tableName,
                            SimpleQueryClauseBase[] clauses)
        {
            _adapter = source._adapter;
            _dataStrategy = source._dataStrategy;
            _tableName = tableName;
            _clauses = clauses;
        }

        public IEnumerable<SimpleQueryClauseBase> Clauses
        {
            get { return _clauses; }
        }

        public string TableName
        {
            get { return _tableName; }
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return Run().GetEnumerator();
        }

        #endregion

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_tempJoinWaitingForOn != null && _tempJoinWaitingForOn.Name.Equals(binder.Name))
            {
                result = _tempJoinWaitingForOn.Table;
            }
            else
            {
                var join = _clauses.OfType<JoinClause>().FirstOrDefault(j => j.Name.Equals(binder.Name));
                if (join != null)
                {
                    result = join.Table;
                }
                else
                {
                    result = new SimpleQuery(this, _tableName + "." + binder.Name,
                                             (SimpleQueryClauseBase[]) _clauses.Clone());
                }
            }
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(IEnumerable<dynamic>))
            {
                result = Cast<dynamic>();
                return true;
            }
            return base.TryConvert(binder, out result);
        }

        /// <summary>
        /// Selects only the specified columns.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <returns>A new <see cref="SimpleQuery"/> which will select only the specified columns.</returns>
        public SimpleQuery Select(params SimpleReference[] columns)
        {
            return Select(columns.AsEnumerable());
        }

        /// <summary>
        /// Selects only the specified columns.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <returns>A new <see cref="SimpleQuery"/> which will select only the specified columns.</returns>
        public SimpleQuery Select(IEnumerable<SimpleReference> columns)
        {
            ThrowIfThereIsAlreadyASelectClause();
            return new SimpleQuery(this, _clauses.Append(new SelectClause(columns)));
        }

        /// <summary>
        /// Selects only the specified columns.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <returns>A new <see cref="SimpleQuery"/> which will select only the specified columns.</returns>
        public SimpleQuery ReplaceSelect(params SimpleReference[] columns)
        {
            return ReplaceSelect(columns.AsEnumerable());
        }

        /// <summary>
        /// Selects only the specified columns.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <returns>A new <see cref="SimpleQuery"/> which will select only the specified columns.</returns>
        public SimpleQuery ReplaceSelect(IEnumerable<SimpleReference> columns)
        {
            return new SimpleQuery(this, _clauses.Where(c => !(c is SelectClause)).Append(new SelectClause(columns)).ToArray());
        }

        private void ThrowIfThereIsAlreadyASelectClause()
        {
            if (_clauses.OfType<SelectClause>().Any())
                throw new InvalidOperationException("Query already contains a Select clause.");
        }

        public SimpleQuery ReplaceWhere(SimpleExpression criteria)
        {
            return new SimpleQuery(this,
                                   _clauses.Where(c => !(c is WhereClause)).Append(new WhereClause(criteria)).ToArray());
        }

        public SimpleQuery Where(SimpleExpression criteria)
        {
            return new SimpleQuery(this, _clauses.Append(new WhereClause(criteria)));
        }

        public SimpleQuery OrderBy(ObjectReference reference, OrderByDirection? direction = null)
        {
            return new SimpleQuery(this, _clauses.Append(new OrderByClause(reference, direction)));
        }

        public SimpleQuery OrderByDescending(ObjectReference reference)
        {
            return new SimpleQuery(this, _clauses.Append(new OrderByClause(reference, OrderByDirection.Descending)));
        }

        public SimpleQuery ThenBy(ObjectReference reference, OrderByDirection? direction = null)
        {
            ThrowIfNoOrderByClause("ThenBy requires an existing OrderBy");

            return new SimpleQuery(this, _clauses.Append(new OrderByClause(reference, direction)));
        }

        private void ThrowIfNoOrderByClause(string message)
        {
            if (!_clauses.OfType<OrderByClause>().Any())
            {
                throw new InvalidOperationException(message);
            }
        }

        public SimpleQuery ThenByDescending(ObjectReference reference)
        {
            if (!_clauses.OfType<OrderByClause>().Any())
            {
                throw new InvalidOperationException("ThenBy requires an existing OrderBy");
            }

            return new SimpleQuery(this, _clauses.Append(new OrderByClause(reference, OrderByDirection.Descending)));
        }

        public SimpleQuery Skip(int skip)
        {
            return new SimpleQuery(this, _clauses.ReplaceOrAppend(new SkipClause(skip)));
        }

        public SimpleQuery ClearSkip()
        {
            return new SimpleQuery(this, _clauses.Where(c => !(c is SkipClause)).ToArray());
        }

        public SimpleQuery Take(int take)
        {
            return new SimpleQuery(this, _clauses.ReplaceOrAppend(new TakeClause(take)));
        }

        public SimpleQuery Distinct()
        {
            return new SimpleQuery(this, _clauses.ReplaceOrAppend(new DistinctClause()));
        }

        public SimpleQuery ClearTake()
        {
            return new SimpleQuery(this, _clauses.Where(c => !(c is TakeClause)).ToArray());
        }

        public SimpleQuery ClearWithTotalCount()
        {
            return new SimpleQuery(this, _clauses.Where(c => !(c is WithCountClause)).ToArray());
        }

        protected IEnumerable<dynamic> Run()
        {
            IEnumerable<SimpleQueryClauseBase> unhandledClauses;
            var result = _adapter.RunQuery(this, out unhandledClauses);

            if (unhandledClauses != null)
            {
                var unhandledClausesList = unhandledClauses.ToList();
                if (unhandledClausesList.Count > 0)
                {
                    result = new DictionaryQueryRunner(result, unhandledClausesList).Run();
                }
            }

            return SimpleResultSet.Create(result, _tableName, _dataStrategy).Cast<dynamic>();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (base.TryInvokeMember(binder, args, out result))
            {
                return true;
            }
            if (binder.Name.StartsWith("order", StringComparison.OrdinalIgnoreCase))
            {
                result = ParseOrderBy(binder.Name);
                return true;
            }
            if (binder.Name.StartsWith("then", StringComparison.OrdinalIgnoreCase))
            {
                result = ParseThenBy(binder.Name);
                return true;
            }
            if (binder.Name.Equals("join", StringComparison.OrdinalIgnoreCase))
            {
                result = args.Length == 1 ? Join(ObjectAsObjectReference(args[0]), JoinType.Inner) : ParseJoin(binder, args);
                return true;
            }
            if (binder.Name.Equals("leftjoin", StringComparison.OrdinalIgnoreCase) || binder.Name.Equals("outerjoin", StringComparison.OrdinalIgnoreCase))
            {
                result = args.Length == 1 ? Join(ObjectAsObjectReference(args[0]), JoinType.Outer) : ParseJoin(binder, args);
                return true;
            }
            if (binder.Name.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                result = ParseOn(binder, args);
                return true;
            }
            if (binder.Name.Equals("having", StringComparison.OrdinalIgnoreCase))
            {
                var expression = args.SingleOrDefault() as SimpleExpression;
                if (expression != null)
                {
                    result = new SimpleQuery(this, _clauses.Append(new HavingClause(expression)));
                    return true;
                }
            }
            if (binder.Name.StartsWith("with", StringComparison.OrdinalIgnoreCase))
            {
                result = ParseWith(binder, args);
                return true;
            }

            return false;
        }

        private SimpleQuery ParseWith(InvokeMemberBinder binder, object[] args)
        {
            if (args.Length > 0)
            {
                if (binder.Name.Equals("with", StringComparison.OrdinalIgnoreCase))
                {
                    return With(args);
                }

                if (binder.Name.Equals("withone", StringComparison.OrdinalIgnoreCase))
                {
                    return With(args, WithType.One);
                }

                if (binder.Name.Equals("withmany", StringComparison.OrdinalIgnoreCase))
                {
                    return With(args, WithType.Many);
                }
            }

            var objectName = binder.Name.Substring(4);
            var withClause = new WithClause(new ObjectReference(objectName, new ObjectReference(_tableName, _dataStrategy), _dataStrategy));
            return new SimpleQuery(this, _clauses.Append(withClause));
        }

        private SimpleQuery With(IEnumerable<object> args, WithType withType = WithType.NotSpecified)
        {
            var clauses = new List<SimpleQueryClauseBase>(_clauses);
            clauses.AddRange(args.OfType<ObjectReference>().Select(reference => new WithClause(reference, withType)));
            return new SimpleQuery(this, clauses.ToArray());
        }

        private ObjectReference ObjectAsObjectReference(object o)
        {
            var objectReference = o as ObjectReference;
            if (!ReferenceEquals(objectReference, null)) return objectReference;

            var dynamicTable = o as DynamicTable;
            if (dynamicTable != null) return new ObjectReference(dynamicTable.GetName(), _dataStrategy);

            throw new InvalidOperationException("Could not convert parameter to ObjectReference.");
        }

        public SimpleQuery Join(ObjectReference objectReference, JoinType joinType)
        {
            if (ReferenceEquals(objectReference, null)) throw new ArgumentNullException("objectReference");
            _tempJoinWaitingForOn = new JoinClause(objectReference, joinType, null);

            return this;
        }

        public SimpleQuery Join(ObjectReference objectReference, out dynamic queryObjectReference)
        {
            return Join(objectReference, JoinType.Inner, out queryObjectReference);
        }

        public SimpleQuery Join(ObjectReference objectReference, JoinType joinType, out dynamic queryObjectReference)
        {
            var newJoin = new JoinClause(objectReference, null);
            _tempJoinWaitingForOn = newJoin;
            queryObjectReference = objectReference;

            return this;
        }

        public SimpleQuery LeftJoin(ObjectReference objectReference)
        {
            return OuterJoin(objectReference);
        }

        public SimpleQuery LeftJoin(ObjectReference objectReference, out dynamic queryObjectReference)
        {
            return OuterJoin(objectReference, out queryObjectReference);
        }

        public SimpleQuery OuterJoin(ObjectReference objectReference)
        {
            if (ReferenceEquals(objectReference, null)) throw new ArgumentNullException("objectReference");
            _tempJoinWaitingForOn = new JoinClause(objectReference, JoinType.Outer);

            return this;
        }

        public SimpleQuery OuterJoin(ObjectReference objectReference, out dynamic queryObjectReference)
        {
            _tempJoinWaitingForOn = new JoinClause(objectReference, JoinType.Outer);
            queryObjectReference = objectReference;

            return this;
        }

        public SimpleQuery On(SimpleExpression joinExpression)
        {
            if (_tempJoinWaitingForOn == null)
                throw new InvalidOperationException("Call to On must be preceded by call to Join.");
            return AddNewJoin(new JoinClause(_tempJoinWaitingForOn.Table, _tempJoinWaitingForOn.JoinType, joinExpression));
        }

        [Obsolete]
        public SimpleQuery WithTotalCount(out Future<int> count)
        {
            Action<int> setCount;
            count = Future<int>.Create(out setCount);
            return new SimpleQuery(this, _clauses.Append(new WithCountClause(setCount)));
        }

        public SimpleQuery WithTotalCount(out Promise<int> count)
        {
            Action<int> setCount;
            count = Promise<int>.Create(out setCount);
            return new SimpleQuery(this, _clauses.Append(new WithCountClause(setCount)));
        }

        private SimpleQuery ParseJoin(InvokeMemberBinder binder, object[] args)
        {
            var tableToJoin = args[0] as ObjectReference;
            if (tableToJoin == null) throw new InvalidOperationException();

            SimpleExpression joinExpression = null;

            if (binder.CallInfo.ArgumentNames.Any(s => !string.IsNullOrWhiteSpace(s)))
            {
                joinExpression = ExpressionHelper.CriteriaDictionaryToExpression(tableToJoin,
                                                                                 binder.NamedArgumentsToDictionary(args));
            }
            else if (args.Length == 2)
            {
                joinExpression = args[1] as SimpleExpression;
            }

            if (joinExpression == null) throw new InvalidOperationException();

            var type = binder.Name.Equals("join", StringComparison.OrdinalIgnoreCase) ? JoinType.Inner : JoinType.Outer;
            var newJoin = new JoinClause(tableToJoin, type, joinExpression);

            return AddNewJoin(newJoin);
        }

        private SimpleQuery ParseOn(InvokeMemberBinder binder, IEnumerable<object> args)
        {
            if (_tempJoinWaitingForOn == null)
                throw new InvalidOperationException("Call to On must be preceded by call to Join.");
            var joinExpression = ExpressionHelper.CriteriaDictionaryToExpression(_tempJoinWaitingForOn.Table,
                                                                                 binder.NamedArgumentsToDictionary(args));
            return AddNewJoin(new JoinClause(_tempJoinWaitingForOn.Table, _tempJoinWaitingForOn.JoinType, joinExpression));
        }

        private SimpleQuery AddNewJoin(JoinClause newJoin)
        {
            _tempJoinWaitingForOn = null;
            return new SimpleQuery(this, _clauses.Append(newJoin));
        }

        private SimpleQuery ParseOrderBy(string methodName)
        {
            methodName = Regex.Replace(methodName, "^order_?by_?", "", RegexOptions.IgnoreCase);
            if (methodName.EndsWith("descending", StringComparison.OrdinalIgnoreCase))
            {
                methodName = Regex.Replace(methodName, "_?descending$", "", RegexOptions.IgnoreCase);
                return OrderByDescending(ObjectReference.FromString(_tableName + "." + methodName));
            }
            return OrderBy(ObjectReference.FromString(_tableName + "." + methodName));
        }

        private SimpleQuery ParseThenBy(string methodName)
        {
            methodName = Regex.Replace(methodName, "^then_?by_?", "", RegexOptions.IgnoreCase);
            if (methodName.EndsWith("descending", StringComparison.OrdinalIgnoreCase))
            {
                methodName = Regex.Replace(methodName, "_?descending$", "", RegexOptions.IgnoreCase);
                return ThenByDescending(ObjectReference.FromString(_tableName + "." + methodName));
            }
            return ThenBy(ObjectReference.FromString(_tableName + "." + methodName));
        }

        public IEnumerable<T> Cast<T>()
        {
            return new CastEnumerable<T>(Run());
        }

        public IEnumerable<T> OfType<T>()
        {
            return new OfTypeEnumerable<T>(Run());
        }

        public IList<dynamic> ToList()
        {
            return Run().ToList();
        }

        public dynamic[] ToArray()
        {
            return Run().ToArray();
        }

        public dynamic ToScalar()
        {
            var data = Run().OfType<IDictionary<string, object>>().ToArray();
            if (data.Length == 0)
            {
                throw new SimpleDataException("Query returned no rows; cannot return scalar value.");
            }
            if (data.Length != 1)
            {
                throw new SimpleDataException("Query returned multiple rows; cannot return scalar value.");
            }
            if (data[0].Count > 1)
            {
                throw new SimpleDataException("Selected row contains multiple values; cannot return scalar value.");
            }
            return data[0].Single().Value;
        }

        public dynamic ToScalarOrDefault()
        {
            var data = Run().OfType<IDictionary<string, object>>().ToArray();
            if (data.Length == 0)
            {
                return null;
            }
            if (data.Length != 1)
            {
                throw new SimpleDataException("Query returned multiple rows; cannot return scalar value.");
            }
            if (data[0].Count > 1)
            {
                throw new SimpleDataException("Selected row contains multiple values; cannot return scalar value.");
            }
            return data[0].Single().Value;
        }

        public IList ToScalarList()
        {
            return ToScalarEnumerable().ToList();
        }

        public dynamic[] ToScalarArray()
        {
            return ToScalarEnumerable().ToArray();
        }

        public IList<T> ToScalarList<T>()
        {
            return ToScalarEnumerable().Cast<T>().ToList();
        }

        public T[] ToScalarArray<T>()
        {
            return ToScalarEnumerable().Cast<T>().ToArray();
        }

        private IEnumerable<dynamic> ToScalarEnumerable()
        {
            return Run().OfType<IDictionary<string, object>>().Select(dict => dict.Values.FirstOrDefault());
        }

        public IList<T> ToList<T>()
        {
            return Cast<T>().ToList();
        }

        public T[] ToArray<T>()
        {
            return Cast<T>().ToArray();
        }

        public T ToScalar<T>()
        {
            return (T) ToScalar();
        }

        public T ToScalarOrDefault<T>()
        {
            return ToScalarOrDefault() ?? default(T);
        }

        public dynamic First()
        {
            return Run().First();
        }

        public dynamic FirstOrDefault()
        {
            return Run().FirstOrDefault();
        }

        public T First<T>()
        {
            return Cast<T>().First();
        }

        public T FirstOrDefault<T>()
        {
            return Cast<T>().FirstOrDefault();
        }

        public T First<T>(Func<T, bool> predicate)
        {
            return Cast<T>().First(predicate);
        }

        public T FirstOrDefault<T>(Func<T, bool> predicate)
        {
            return Cast<T>().FirstOrDefault(predicate);
        }

        public dynamic Single()
        {
            return Run().First();
        }

        public dynamic SingleOrDefault()
        {
            return Run().FirstOrDefault();
        }

        public T Single<T>()
        {
            return Cast<T>().Single();
        }

        public T SingleOrDefault<T>()
        {
            return Cast<T>().SingleOrDefault();
        }

        public T Single<T>(Func<T, bool> predicate)
        {
            return Cast<T>().Single(predicate);
        }

        public T SingleOrDefault<T>(Func<T, bool> predicate)
        {
            return Cast<T>().SingleOrDefault(predicate);
        }

        public int Count()
        {
            return (int)Select(new CountSpecialReference()).ToScalar();
        }

        /// <summary>
        /// Checks whether the query matches any records without running the full query.
        /// </summary>
        /// <returns><c>true</c> if the query matches any record; otherwise, <c>false</c>.</returns>
        public bool Exists()
        {
            return Select(new ExistsSpecialReference()).Run().Count() == 1;
        }

        /// <summary>
        /// Checks whether the query matches any records without running the full query.
        /// </summary>
        /// <returns><c>true</c> if the query matches any record; otherwise, <c>false</c>.</returns>
        /// <remarks>This method is an alias for <see cref="Exists"/>.</remarks>
        public bool Any()
        {
            return Exists();
        }

        public void SetDataStrategy(DataStrategy dataStrategy)
        {
            _dataStrategy = dataStrategy;
        }

        public IObservable<dynamic> AsObservable()
        {
            if (_asObservableImplementation != null) return _asObservableImplementation();

            try
            {
                _asObservableImplementation = () =>
                                                  {
                                                      IEnumerable<SimpleQueryClauseBase> unhandledClauses;
                                                      return _adapter.RunQueryAsObservable(this, out unhandledClauses)
                                                          .Map(d => new SimpleRecord(d, _tableName, _dataStrategy));
                                                  };
                return _asObservableImplementation();
            }
            catch (NotImplementedException)
            {
                _asObservableImplementation =
                    () => Run().Select(d => new SimpleRecord(d, _tableName, _dataStrategy)).ToObservable();
            }

            return _asObservableImplementation();
        }

        public Task<IEnumerable<dynamic>> RunTask()
        {
            return Task.Factory.StartNew<IEnumerable<dynamic>>(Run);
        }

        public SimpleQuery ClearOrderBy()
        {
            return new SimpleQuery(this, _clauses.Where(c => !(c is OrderByClause)).ToArray());
        }

        public SimpleQuery ClearWith()
        {
            return new SimpleQuery(this, _clauses.Where(c => !(c is WithClause)).ToArray());
        }
    }
}