namespace Simple.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Commands;
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

            var collectionType = binder.Type.GetInterface("ICollection`1");
            if (collectionType != null)
            {
                if (TryConvertToGenericCollection(binder, out result, collectionType)) return true;
            }

            if (binder.Type.Name.Equals("IEnumerable`1"))
            {
                var genericArguments = binder.Type.GetGenericArguments();
                var cast =
                    typeof (SimpleQuery).GetMethod("Cast").MakeGenericMethod(genericArguments);
                result = cast.Invoke(this, null);
                return true;
            }

            return base.TryConvert(binder, out result);
        }

        private bool TryConvertToGenericCollection(ConvertBinder binder, out object result, Type collectionType)
        {
            var genericArguments = collectionType.GetGenericArguments();
            var enumerableConstructor =
                binder.Type.GetConstructor(new[]
                                               {
                                                   typeof (IEnumerable<>).MakeGenericType(
                                                       genericArguments)
                                               });
            if (enumerableConstructor != null)
            {
                var cast =
                    typeof (SimpleQuery).GetMethod("Cast").MakeGenericMethod(genericArguments);
                result = Activator.CreateInstance(binder.Type, cast.Invoke(this, null));
                return true;
            }

            var defaultConstructor = binder.Type.GetConstructor(new Type[0]);
            if (defaultConstructor != null)
            {
                result = Activator.CreateInstance(binder.Type);
                var add = binder.Type.GetMethod("Add", genericArguments);
                var cast =
                    typeof (SimpleQuery).GetMethod("Cast").MakeGenericMethod(genericArguments);
                foreach (var item in (IEnumerable) cast.Invoke(this, null))
                {
                    add.Invoke(result, new[] {item});
                }
                return true;
            }
            result = null;
            return false;
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

        /// <summary>
        /// Alters the query to lock the rows for update. 
        /// </summary>
        /// <param name="skipLockedRows">Indicates whether the selection should skip rows already locked</param>
        /// <returns>A new <see cref="SimpleQuery"/> which will perform locking on the selected rows</returns>
        public SimpleQuery ForUpdate(bool skipLockedRows)
        {
            return new SimpleQuery(this, _clauses.Where(c => !(c is ForUpdateClause)).Append(new ForUpdateClause(skipLockedRows)).ToArray());
        }

        /// <summary>
        /// Removes any specified ForUpdate from the Query
        /// </summary>
        /// <returns>A new <see cref="SimpleQuery"/> with any specified ForUpdate removed</returns>
        public SimpleQuery ClearForUpdate()
        {
            return new SimpleQuery(this, _clauses.Where(c => !(c is ForUpdateClause)).ToArray());
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
            if (criteria == null) throw new ArgumentNullException("criteria");
            return new SimpleQuery(this, _clauses.Append(new WhereClause(criteria)));
        }

        public SimpleQuery OrderBy(ObjectReference reference, OrderByDirection? direction = null)
        {
            return new SimpleQuery(this, _clauses.Append(new OrderByClause(reference, direction)));
        }

        public SimpleQuery OrderBy(params ObjectReference[] references)
        {
            if (references.Length == 0)
            {
                throw new ArgumentException("OrderBy requires parameters");
            }
            var q = this.OrderBy(references[0]);
            foreach (var reference in references.Skip(1))
            {
                q = q.ThenBy(reference);
            }
            return q;
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
            if (_clauses == null || !_clauses.OfType<OrderByClause>().Any())
            {
                throw new InvalidOperationException(message);
            }
        }

        public SimpleQuery ThenByDescending(ObjectReference reference)
        {
            return ThenBy(reference, OrderByDirection.Descending);
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
            var result = _dataStrategy.Run.RunQuery(this, out unhandledClauses);

            if (unhandledClauses != null)
            {
                var unhandledClausesList = unhandledClauses.ToList();
                if (unhandledClausesList.Count > 0)
                {
                    result = new DictionaryQueryRunner(_tableName, result, unhandledClausesList).Run();
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
                if (args.Length != 0)
                {
                    throw new ArgumentException("OrderByColumn form does not accept parameters");
                }
                result = ParseOrderBy(binder.Name);
                return true;
            }
            if (binder.Name.StartsWith("then", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length != 0)
                {
                    throw new ArgumentException("ThenByColumn form does not accept parameters");
                }
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
            if (binder.Name.StartsWith("with", StringComparison.OrdinalIgnoreCase) && !binder.Name.Equals("WithTotalCount", StringComparison.OrdinalIgnoreCase))
            {
                result = ParseWith(binder, args);
                return true;
            }
            if (binder.Name.Equals("select", StringComparison.OrdinalIgnoreCase))
            {
                result = Select(args.OfType<SimpleReference>());
                return true;
            }

            var command = Commands.CommandFactory.GetCommandFor(binder.Name) as IQueryCompatibleCommand;
            if (command != null)
            {
                try
                {
                    result = command.Execute(_dataStrategy, this, binder, args);
                    return true;
                }
                catch (NotImplementedException)
                {
                }
            }
            try
            {
                var methodInfo = typeof(SimpleQuery).GetMethod(binder.Name, args.Select(a => (a ?? new object()).GetType()).ToArray());
                if (methodInfo != null)
                {
                    methodInfo.Invoke(this, args);
                }
            }
            catch (AmbiguousMatchException)
            {
            }

            if (binder.Name.Equals("where", StringComparison.InvariantCultureIgnoreCase) || binder.Name.Equals("replacewhere", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new BadExpressionException("Where methods require a single criteria expression.");
            }

            return false;
        }

        public SimpleQuery With(ObjectReference reference, out dynamic queryObjectReference)
        {
            queryObjectReference = reference;
            return With(new[] {reference});
        }

        public SimpleQuery WithOne(ObjectReference reference, out dynamic queryObjectReference)
        {
            queryObjectReference = reference;
            return With(new[] {reference}, WithType.One);
        }

        public SimpleQuery WithMany(ObjectReference reference, out dynamic queryObjectReference)
        {
            queryObjectReference = reference;
            return With(new[] {reference}, WithType.Many);
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
            if (string.IsNullOrWhiteSpace(objectName))
            {
                throw new ArgumentException("With requires a Table reference");
            }
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

        public SimpleQuery Join(DynamicTable dynamicTable, JoinType joinType)
        {
            if (ReferenceEquals(dynamicTable, null)) throw new ArgumentNullException("dynamicTable");
            _tempJoinWaitingForOn = new JoinClause(dynamicTable.ToObjectReference(), joinType, null);

            return this;
        }

        public SimpleQuery Join(DynamicTable dynamicTable, out dynamic queryObjectReference)
        {
            return Join(dynamicTable, JoinType.Inner, out queryObjectReference);
        }

        public SimpleQuery Join(DynamicTable dynamicTable, JoinType joinType, out dynamic queryObjectReference)
        {
            if (ReferenceEquals(dynamicTable, null)) throw new ArgumentNullException("dynamicTable");
            var newJoin = new JoinClause(dynamicTable.ToObjectReference(), null);
            _tempJoinWaitingForOn = newJoin;
            queryObjectReference = dynamicTable.ToObjectReference();

            return this;
        }

        public SimpleQuery LeftJoin(DynamicTable dynamicTable)
        {
            return OuterJoin(dynamicTable);
        }

        public SimpleQuery LeftJoin(DynamicTable dynamicTable, out dynamic queryObjectReference)
        {
            return OuterJoin(dynamicTable, out queryObjectReference);
        }

        public SimpleQuery OuterJoin(DynamicTable dynamicTable)
        {
            if (ReferenceEquals(dynamicTable, null)) throw new ArgumentNullException("dynamicTable");
            _tempJoinWaitingForOn = new JoinClause(dynamicTable.ToObjectReference(), JoinType.Outer);

            return this;
        }

        public SimpleQuery OuterJoin(DynamicTable dynamicTable, out dynamic queryObjectReference)
        {
            _tempJoinWaitingForOn = new JoinClause(dynamicTable.ToObjectReference(), JoinType.Outer);
            queryObjectReference = dynamicTable;

            return this;
        }

        public SimpleQuery On(SimpleExpression joinExpression)
        {
            if (_tempJoinWaitingForOn == null)
            {
                throw new InvalidOperationException("Call to On must be preceded by call to JoinInfo.");
            }
            if (ReferenceEquals(joinExpression, null))
            {
                throw new BadExpressionException("On expects an expression or named parameters.");
            }
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
            if (ReferenceEquals(tableToJoin, null))
            {
                var dynamicTable = args[0] as DynamicTable;
                if (!ReferenceEquals(dynamicTable, null))
                {
                    tableToJoin = dynamicTable.ToObjectReference();
                }
            }
            if (tableToJoin == null) throw new BadJoinExpressionException("Incorrect join table specified");
            if (HomogenizedEqualityComparer.DefaultInstance.Equals(tableToJoin.GetAliasOrName(), _tableName))
            {
                throw new BadJoinExpressionException("Cannot join unaliased table to itself.");
            }

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

            if (joinExpression == null) throw new BadJoinExpressionException("Could not create join expression");

            var type = binder.Name.Equals("join", StringComparison.OrdinalIgnoreCase) ? JoinType.Inner : JoinType.Outer;
            var newJoin = new JoinClause(tableToJoin, type, joinExpression);

            return AddNewJoin(newJoin);
        }

        private SimpleQuery ParseOn(InvokeMemberBinder binder, IEnumerable<object> args)
        {
            if (_tempJoinWaitingForOn == null)
            {
                throw new InvalidOperationException("Call to On must be preceded by call to JoinInfo.");
            }
            var namedArguments = binder.NamedArgumentsToDictionary(args);
            if (namedArguments == null || namedArguments.Count == 0)
            {
                throw new BadExpressionException("On expects an expression or named parameters.");
            }
            var joinExpression = ExpressionHelper.CriteriaDictionaryToExpression(_tempJoinWaitingForOn.Table,
                                                                                 namedArguments);
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
            if (string.IsNullOrWhiteSpace(methodName))
            {
                throw new ArgumentException("Invalid arguments to OrderBy");
            }
            if (methodName.EndsWith("descending", StringComparison.OrdinalIgnoreCase))
            {
                methodName = Regex.Replace(methodName, "_?descending$", "", RegexOptions.IgnoreCase);
                if (string.IsNullOrWhiteSpace(methodName))
                {
                    throw new ArgumentException("Invalid arguments to OrderByDescending");
                }
                return OrderByDescending(ObjectReference.FromString(_tableName + "." + methodName));
            }
            return OrderBy(ObjectReference.FromString(_tableName + "." + methodName));
        }

        private SimpleQuery ParseThenBy(string methodName)
        {
            ThrowIfNoOrderByClause("Must call OrderBy before ThenBy");
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
            if (data[0].Count == 0)
            {
                throw new SimpleDataException("Selected row contains no values; cannot return scalar value.");
            }
            return data[0].First().Value;
        }

        public dynamic ToScalarOrDefault()
        {
            var data = Run().OfType<IDictionary<string, object>>().ToArray();
            if (data.Length == 0)
            {
                return null;
            }
            if (data[0].Count == 0)
            {
                return null;
            }
            return data[0].First().Value;
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
            return Take(1).Run().First();
        }

        public dynamic FirstOrDefault()
        {
            return Take(1).Run().FirstOrDefault();
        }

        public T First<T>()
        {
            return Take(1).Cast<T>().First();
        }

        public T FirstOrDefault<T>()
        {
            return Take(1).Cast<T>().FirstOrDefault();
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
            return Take(1).Run().First();
        }

        public dynamic SingleOrDefault()
        {
            return Take(1).Run().FirstOrDefault();
        }

        public T Single<T>()
        {
            return Take(1).Cast<T>().Single();
        }

        public T SingleOrDefault<T>()
        {
            return Take(1).Cast<T>().SingleOrDefault();
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
            return Convert.ToInt32(Select(new CountSpecialReference()).ToScalar());
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