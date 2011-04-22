using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
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
    }
}
