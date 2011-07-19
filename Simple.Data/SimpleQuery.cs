using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp.RuntimeBinder;
using Simple.Data.Extensions;

namespace Simple.Data
{
    using System.Threading.Tasks;

    public class SimpleQuery : DynamicObject, IEnumerable
    {
        private DataStrategy _dataStrategy;
        private readonly Adapter _adapter;
        private readonly string _tableName;

        private readonly SimpleQueryClauseBase[] _clauses;
        private JoinClause _tempJoinWaitingForOn;

        public SimpleQuery(Adapter adapter, string tableName)
        {
            _adapter = adapter;
            _tableName = tableName;
            _clauses = new SimpleQueryClauseBase[0];
        }

        private SimpleQuery(SimpleQuery source,
            SimpleQueryClauseBase[] clauses)
        {
            _adapter = source._adapter;
            _tableName = source.TableName;
            _clauses = clauses;
        }

        private SimpleQuery(SimpleQuery source,
            string tableName,
            SimpleQueryClauseBase[] clauses)
        {
            _adapter = source._adapter;
            _tableName = tableName;
            _clauses = clauses;
        }

        public SimpleExpression HavingCriteria
        {
            get
            {
                var havingClause = _clauses.OfType<HavingClause>().SingleOrDefault();
                return havingClause == null ? null : havingClause.Criteria;
            }
        }

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

        public IEnumerable<JoinClause> Joins
        {
            get { return _clauses.OfType<JoinClause>(); }
        }

        public IEnumerable<SimpleReference> Columns
        {
            get
            {
                var selectClause = _clauses.OfType<SelectClause>().SingleOrDefault();
                return selectClause == null ? Enumerable.Empty<SimpleReference>() : selectClause.Columns;
            }
        }

        public int? TakeCount
        {
            get
            {
                var takeClause = _clauses.OfType<TakeClause>().SingleOrDefault();
                return takeClause == null ? null : (int?)takeClause.Count;
            }
        }

        public int? SkipCount
        {
            get
            {
                var skipClause = _clauses.OfType<SkipClause>().SingleOrDefault();
                return skipClause == null ? null : (int?)skipClause.Count;
            }
        }

        public IEnumerable<OrderByClause> Order
        {
            get { return _clauses.OfType<OrderByClause>(); }
        }

        public SimpleExpression Criteria
        {
            get
            {
                var whereClause = _clauses.OfType<WhereClause>().SingleOrDefault();
                return whereClause == null ? null : whereClause.Criteria;
            }
        }

        public string TableName
        {
            get { return _tableName; }
        }

        /// <summary>
        /// Selects only the specified columns.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <returns>A new <see cref="SimpleQuery"/> which will select only the specified columns.</returns>
        public SimpleQuery Select(params SimpleReference[] columns)
        {
            return new SimpleQuery(this, _clauses.Append(new SelectClause(columns)));
        }

        /// <summary>
        /// Selects only the specified columns.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <returns>A new <see cref="SimpleQuery"/> which will select only the specified columns.</returns>
        public SimpleQuery Select(IEnumerable<SimpleReference> columns)
        {
            return new SimpleQuery(this, _clauses.Append(new SelectClause(columns)));
        }

        public SimpleQuery ReplaceWhere(SimpleExpression criteria)
        {
            return new SimpleQuery(this, _clauses.Append(new WhereClause(criteria)));
        }

        public SimpleQuery Where(SimpleExpression criteria)
        {
            var currentWhere = _clauses.OfType<WhereClause>().SingleOrDefault();
            if (currentWhere == null)
            {
                return new SimpleQuery(this, _clauses.Append(new WhereClause(criteria)));
            }

            var index = Array.IndexOf(_clauses, currentWhere);

            return new SimpleQuery(this, _clauses.Replace(index, new WhereClause(currentWhere.Criteria && criteria)));
        }

        public SimpleQuery OrderBy(ObjectReference reference)
        {
            return new SimpleQuery(this, _clauses.Append(new OrderByClause(reference)));
        }

        public SimpleQuery OrderByDescending(ObjectReference reference)
        {
            return new SimpleQuery(this, _clauses.Append(new OrderByClause(reference, OrderByDirection.Descending)));
        }

        public SimpleQuery ThenBy(ObjectReference reference)
        {
            if (!_clauses.OfType<OrderByClause>().Any())
            {
                throw new InvalidOperationException("ThenBy requires an existing OrderBy");
            }

            return new SimpleQuery(this, _clauses.Append(new OrderByClause(reference)));
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
            return new SimpleQuery(this, _clauses.Append(new SkipClause(skip)));
        }

        public SimpleQuery Take(int take)
        {
            return new SimpleQuery(this, _clauses.Append(new TakeClause(take)));
        }

        protected IEnumerable<dynamic> Run()
        {
            return _adapter.RunQuery(this).Select(d => new SimpleRecord(d, _tableName, _dataStrategy));
        }

        public IEnumerator GetEnumerator()
        {
            return Run().GetEnumerator();
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
                result = args.Length == 1 ? (object)Join(args[0] as ObjectReference) : ParseJoin(binder, args);
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

            return false;
        }

        public SimpleQuery Join(ObjectReference objectReference)
        {
            _tempJoinWaitingForOn = new JoinClause(objectReference, null);

            return this;
        }

        public SimpleQuery Join(ObjectReference objectReference, out dynamic queryObjectReference)
        {
            var newJoin = new JoinClause(objectReference, null);
            _tempJoinWaitingForOn = newJoin;
            queryObjectReference = objectReference;

            return this;
        }

        public SimpleQuery On(SimpleExpression joinExpression)
        {
            if (_tempJoinWaitingForOn == null) throw new InvalidOperationException("Call to On must be preceded by call to Join.");
            return AddNewJoin(new JoinClause(_tempJoinWaitingForOn.Table, joinExpression));
        }

        private SimpleQuery ParseJoin(InvokeMemberBinder binder, object[] args)
        {
            var tableToJoin = args[0] as ObjectReference;
            if (tableToJoin == null) throw new InvalidOperationException();

            SimpleExpression joinExpression = null;

            if (binder.CallInfo.ArgumentNames.Any(s => !string.IsNullOrWhiteSpace(s)))
            {
                joinExpression = ExpressionHelper.CriteriaDictionaryToExpression(tableToJoin, binder.NamedArgumentsToDictionary(args));
            }
            else if (args.Length == 2)
            {
                joinExpression = args[1] as SimpleExpression;
            }

            if (joinExpression == null) throw new InvalidOperationException();

            var newJoin = new JoinClause(tableToJoin, joinExpression);

            return AddNewJoin(newJoin);
        }

        private SimpleQuery ParseOn(InvokeMemberBinder binder, object[] args)
        {
            if (_tempJoinWaitingForOn == null) throw new InvalidOperationException("Call to On must be preceded by call to Join.");
            var joinExpression = ExpressionHelper.CriteriaDictionaryToExpression(_tempJoinWaitingForOn.Table,
                                                                                 binder.NamedArgumentsToDictionary(args));
            return new SimpleQuery(this, _clauses.Append(new JoinClause(_tempJoinWaitingForOn.Table, joinExpression)));
        }

        private SimpleQuery AddNewJoin(JoinClause newJoin)
        {
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
            return Run().Select(item => (T)item);
        }

        public IEnumerable<T> OfType<T>()
        {
            foreach (var item in Run())
            {
                bool success = true;
                T cast;
                try
                {
                    cast = (T)(object)item;
                }
                catch (RuntimeBinderException)
                {
                    cast = default(T);
                    success = false;
                }
                if (success)
                {
                    yield return cast;
                }
            }
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
            var data = _adapter.RunQuery(this).ToArray();
            if (data.Length != 1)
            {
                throw new SimpleDataException("Query returned multiple rows; cannot return scalar value.");
            }
            if (data[0].Count == 0)
            {
                throw new SimpleDataException("Query returned no rows; cannot return scalar value.");
            }
            if (data[0].Count > 1)
            {
                throw new SimpleDataException("Selected row contains multiple values; cannot return scalar value.");
            }
            return data[0].Single().Value;
        }

        public dynamic ToScalarOrDefault()
        {
            var data = _adapter.RunQuery(this).ToArray();
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
            return _adapter.RunQuery(this).Select(dict => dict.Values.FirstOrDefault());
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
            return this.Select(new CountSpecialReference()).ToScalar();
        }

        /// <summary>
        /// Checks whether the query matches any records without running the full query.
        /// </summary>
        /// <returns><c>true</c> if the query matches any record; otherwise, <c>false</c>.</returns>
        public bool Exists()
        {
            return _adapter.RunQuery(this.Select(new ExistsSpecialReference())).Count() == 1;
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
                _asObservableImplementation = () => _adapter.RunQueryAsObservable(this).Map(d => new SimpleRecord(d, _tableName, _dataStrategy));
                return _asObservableImplementation();
            }
            catch (NotImplementedException)
            {
                _asObservableImplementation = () => _adapter.RunQuery(this).Select(d => new SimpleRecord(d, _tableName, _dataStrategy)).ToObservable();
            }

            return _asObservableImplementation();
        }

        private Func<IObservable<dynamic>> _asObservableImplementation;

        public Task<IEnumerable<dynamic>> RunTask()
        {
            return Task.Factory.StartNew<IEnumerable<dynamic>>(Run);
        }
    }
}
