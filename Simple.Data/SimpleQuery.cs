using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Simple.Data.Extensions;

namespace Simple.Data
{
    public class SimpleQuery : DynamicObject, IEnumerable
    {
        private readonly Adapter _adapter;
        private readonly string _tableName;
        private readonly SimpleExpression _criteria;
        private readonly IEnumerable<SimpleOrderByItem> _order;
        private readonly int? _skipCount;
        private readonly int? _takeCount;

        private readonly object _sync = new object();
        private IEnumerable<dynamic> _records;

        public SimpleQuery(Adapter adapter, string tableName)
        {
            _adapter = adapter;
            _tableName = tableName;
        }

        private SimpleQuery(SimpleQuery source,
            string tableName = null,
            SimpleExpression criteria = null,
            IEnumerable<SimpleOrderByItem> order = null,
            int? skipCount = null,
            int? takeCount = null)
        {
            _adapter = source._adapter;
            _tableName = tableName ?? source.TableName;
            _criteria = criteria ?? source.Criteria;
            _order = order ?? source.Order;
            _skipCount = skipCount ?? source.SkipCount;
            _takeCount = takeCount ?? source.TakeCount;
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

        public SimpleQuery Where(SimpleExpression criteria)
        {
            return new SimpleQuery(this, criteria: criteria);
        }

        public SimpleQuery OrderBy(DynamicReference reference)
        {
            return new SimpleQuery(this, order: new[] {new SimpleOrderByItem(reference)});
        }

        public SimpleQuery OrderByDescending(DynamicReference reference)
        {
            return new SimpleQuery(this, order: new[] { new SimpleOrderByItem(reference, OrderByDirection.Descending) });
        }

        public SimpleQuery ThenBy(DynamicReference reference)
        {
            if (_order == null)
            {
                throw new InvalidOperationException("ThenBy requires an existing OrderBy");
            }

            return new SimpleQuery(this, order: _order.Append(new SimpleOrderByItem(reference)));
        }

        public SimpleQuery ThenByDescending(DynamicReference reference)
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

        public IEnumerator GetEnumerator()
        {
            if (_records == null)
            {
                lock (_sync)
                {
                    if (_records == null)
                    {
                        _records = _adapter.RunQuery(this).Select(d => new SimpleRecord(d, _tableName));
                    }
                }
            }

            return _records.GetEnumerator();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
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
            return base.TryInvokeMember(binder, args, out result);
        }

        private SimpleQuery ParseOrderBy(string methodName)
        {
            methodName = Regex.Replace(methodName, "^order_?by_?", "", RegexOptions.IgnoreCase);
            if (methodName.EndsWith("descending", StringComparison.OrdinalIgnoreCase))
            {
                methodName = Regex.Replace(methodName, "_?descending$", "", RegexOptions.IgnoreCase);
                return OrderByDescending(DynamicReference.FromString(_tableName + "." + methodName));
            }
            return OrderBy(DynamicReference.FromString(_tableName + "." + methodName));
        }

        private SimpleQuery ParseThenBy(string methodName)
        {
            methodName = Regex.Replace(methodName, "^then_?by_?", "", RegexOptions.IgnoreCase);
            if (methodName.EndsWith("descending", StringComparison.OrdinalIgnoreCase))
            {
                methodName = Regex.Replace(methodName, "_?descending$", "", RegexOptions.IgnoreCase);
                return ThenByDescending(DynamicReference.FromString(_tableName + "." + methodName));
            }
            return ThenBy(DynamicReference.FromString(_tableName + "." + methodName));
        }
    }
}
