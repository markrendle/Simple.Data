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
    public class SimpleQuery : DynamicObject, IEnumerable
    {
        private DataStrategy _dataStrategy;
        private readonly Adapter _adapter;
        private readonly string _tableName;
        private readonly IEnumerable<SimpleReference> _columns;
        private readonly SimpleExpression _criteria;
        private readonly SimpleExpression _havingCriteria;
        private readonly IEnumerable<SimpleOrderByItem> _order;
        internal IDictionary<string, SimpleQueryJoin> _joins; 
        private readonly int? _skipCount;
        private readonly int? _takeCount;
        private SimpleQueryJoin _tempJoinWaitingForOn;

        public SimpleQuery(Adapter adapter, string tableName)
        {
            _adapter = adapter;
            _tableName = tableName;
        }

        private SimpleQuery(SimpleQuery source,
            string tableName = null,
            IDictionary<string,SimpleQueryJoin> joins = null,
            IEnumerable<SimpleReference> columns = null,
            SimpleExpression criteria = null,
            SimpleExpression having = null,
            IEnumerable<SimpleOrderByItem> order = null,
            int? skipCount = null,
            int? takeCount = null)
        {
            _adapter = source._adapter;
            _tableName = tableName ?? source.TableName;
            _joins = joins ?? source._joins;
            _columns = columns ?? source.Columns ?? Enumerable.Empty<SimpleReference>();
            _criteria = criteria ?? source.Criteria;
            _havingCriteria = having ?? source.HavingCriteria;
            _order = order ?? source.Order;
            _skipCount = skipCount ?? source.SkipCount;
            _takeCount = takeCount ?? source.TakeCount;
        }

        public SimpleExpression HavingCriteria
        {
            get { return _havingCriteria; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_joins != null && _joins.ContainsKey(binder.Name))
            {
                result = _joins[binder.Name].Table;
            }
            else
            {
                result = new SimpleQuery(this, _tableName + "." + binder.Name);
            }
            return true;
        }

        public IEnumerable<SimpleQueryJoin> Joins
        {
            get { return _joins != null ? _joins.Values : Enumerable.Empty<SimpleQueryJoin>(); }
        }

        public IEnumerable<SimpleReference> Columns
        {
            get { return _columns ?? Enumerable.Empty<SimpleReference>(); }
        }

        public int? TakeCount
        {
            get { return _takeCount; }
        }

        public int? SkipCount
        {
            get { return _skipCount; }
        }

        public IEnumerable<SimpleOrderByItem> Order
        {
            get { return _order; }
        }

        public SimpleExpression Criteria
        {
            get { return _criteria; }
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
            return new SimpleQuery(this, columns: columns);
        }

        /// <summary>
        /// Selects only the specified columns.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <returns>A new <see cref="SimpleQuery"/> which will select only the specified columns.</returns>
        public SimpleQuery Select(IEnumerable<SimpleReference> columns)
        {
            return new SimpleQuery(this, columns: columns);
        }

        public SimpleQuery ReplaceWhere(SimpleExpression criteria)
        {
            return new SimpleQuery(this, criteria: criteria);
        }

        public SimpleQuery Where(SimpleExpression criteria)
        {
            return _criteria == null
                       ? new SimpleQuery(this, criteria: criteria)
                       : new SimpleQuery(this,
                                         criteria: new SimpleExpression(_criteria, criteria, SimpleExpressionType.And));
        }

        public SimpleQuery OrderBy(ObjectReference reference)
        {
            return new SimpleQuery(this, order: new[] {new SimpleOrderByItem(reference)});
        }

        public SimpleQuery OrderByDescending(ObjectReference reference)
        {
            return new SimpleQuery(this, order: new[] { new SimpleOrderByItem(reference, OrderByDirection.Descending) });
        }

        public SimpleQuery ThenBy(ObjectReference reference)
        {
            if (_order == null)
            {
                throw new InvalidOperationException("ThenBy requires an existing OrderBy");
            }

            return new SimpleQuery(this, order: _order.Append(new SimpleOrderByItem(reference)));
        }

        public SimpleQuery ThenByDescending(ObjectReference reference)
        {
            if (_order == null)
            {
                throw new InvalidOperationException("ThenBy requires an existing OrderBy");
            }

            return new SimpleQuery(this, order: _order.Append(new SimpleOrderByItem(reference, OrderByDirection.Descending)));
        }

        public SimpleQuery Skip(int skip)
        {
            return new SimpleQuery(this, skipCount: skip);
        }

        public SimpleQuery Take(int take)
        {
            return new SimpleQuery(this, takeCount: take);
        }

        protected IEnumerable<dynamic> Records
        {
            get
            {
                return _adapter.RunQuery(this).Select(d => new SimpleRecord(d, _tableName, _dataStrategy));
            }
        }

        public IEnumerator GetEnumerator()
        {
            return Records.GetEnumerator();
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
                var clause = args.SingleOrDefault() as SimpleExpression;
                if (clause != null)
                {
                    result = new SimpleQuery(this, having: clause);
                    return true;
                }
            }

            return false;
        }

        public SimpleQuery Join(ObjectReference objectReference)
        {
            var newJoin = new SimpleQueryJoin(objectReference, null);
            if (_joins == null) _joins = new Dictionary<string, SimpleQueryJoin>();
            _joins.Add(newJoin.Name, newJoin);
            _tempJoinWaitingForOn = newJoin;

            return this;
        }

        public SimpleQuery Join(ObjectReference objectReference, out dynamic queryObjectReference)
        {
            var newJoin = new SimpleQueryJoin(objectReference, null);
            if (_joins == null) _joins = new Dictionary<string, SimpleQueryJoin>();
            _joins.Add(newJoin.Name, newJoin);
            _tempJoinWaitingForOn = newJoin;
            queryObjectReference = objectReference;

            return this;
        }

        public SimpleQuery On(SimpleExpression joinExpression)
        {
            if (_tempJoinWaitingForOn == null) throw new InvalidOperationException("Call to On must be preceded by call to Join.");
            _joins.Remove(_tempJoinWaitingForOn.Name);
            return AddNewJoin(new SimpleQueryJoin(_tempJoinWaitingForOn.Table, joinExpression));
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

            var newJoin = new SimpleQueryJoin(tableToJoin, joinExpression);

            return AddNewJoin(newJoin);
        }

        private SimpleQuery ParseOn(InvokeMemberBinder binder, object[] args)
        {
            if (_tempJoinWaitingForOn == null) throw new InvalidOperationException("Call to On must be preceded by call to Join.");
            _joins.Remove(_tempJoinWaitingForOn.Name);
            var joinExpression = ExpressionHelper.CriteriaDictionaryToExpression(_tempJoinWaitingForOn.Table,
                                                                                 binder.NamedArgumentsToDictionary(args));
            return AddNewJoin(new SimpleQueryJoin(_tempJoinWaitingForOn.Table, joinExpression));
        }

        private SimpleQuery AddNewJoin(SimpleQueryJoin newJoin)
        {
            var newJoinsDictionary = _joins != null
                                         ? new Dictionary<string, SimpleQueryJoin>(_joins)
                                         : new Dictionary<string, SimpleQueryJoin>();

            var joinName = newJoin.Table.Alias ?? newJoin.Table.GetName();
            newJoinsDictionary[joinName] = newJoin;

            return new SimpleQuery(this, joins: newJoinsDictionary);
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
            return Records.Select(item => (T)item);
        }

        public IEnumerable<T> OfType<T>()
        {
            foreach (var item in Records)
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
            return Records.ToList();
        }

        public dynamic[] ToArray()
        {
            return Records.ToArray();
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
            return Records.First();
        }

        public dynamic FirstOrDefault()
        {
            return Records.FirstOrDefault();
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
            return Records.First();
        }

        public dynamic SingleOrDefault()
        {
            return Records.FirstOrDefault();
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
    }

    public class SimpleQueryJoinHolder
    {
        private readonly SimpleQuery _query;
        private readonly string _joinName;

        public SimpleQueryJoinHolder(SimpleQuery query, string joinName)
        {
            _query = query;
            _joinName = joinName;
        }

        public SimpleQuery On(SimpleExpression expression)
        {
            var join = _query._joins[_joinName];
            _query._joins[_joinName] = new SimpleQueryJoin(join.Table, expression);
            return _query;
        }
    }
}
