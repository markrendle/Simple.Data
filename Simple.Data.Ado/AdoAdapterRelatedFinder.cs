using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    class AdoAdapterRelatedFinder
    {
        private readonly Lazy<ConcurrentDictionary<Tuple<string, string>, TableJoin>> _tableJoins =
            new Lazy<ConcurrentDictionary<Tuple<string, string>, TableJoin>>(
                () => new ConcurrentDictionary<Tuple<string, string>, TableJoin>(), LazyThreadSafetyMode.ExecutionAndPublication
                );

        private readonly AdoAdapter _adapter;

        public AdoAdapterRelatedFinder(AdoAdapter adapter)
        {
            _adapter = adapter;
        }

        public bool IsValidRelation(string tableName, string relatedTableName)
        {
            return TryJoin(tableName, relatedTableName) != null;
        }

        public object FindRelated(string tableName, IDictionary<string, object> row, string relatedTableName)
        {
            var join = TryJoin(tableName, relatedTableName);
            if (join == null) throw new AdoAdapterException("Could not resolve relationship.");

            if (join.Master == _adapter.GetSchema().FindTable(tableName))
            {
                return GetDetail(row, join);
            }
            return GetMaster(row, join);
        }

        private TableJoin TryJoin(string tableName, string relatedTableName)
        {
            return _tableJoins.Value.GetOrAdd(Tuple.Create(tableName, relatedTableName),
                                              t => TryCreateJoin(t.Item1, t.Item2));
        }

        private TableJoin TryCreateJoin(string tableName, string relatedTableName)
        {
            return TryMasterJoin(tableName, relatedTableName) ?? TryDetailJoin(tableName, relatedTableName);
        }

        private TableJoin TryMasterJoin(string tableName, string relatedTableName)
        {
            return _adapter.GetSchema().FindTable(tableName).GetMaster(relatedTableName);
        }

        private TableJoin TryDetailJoin(string tableName, string relatedTableName)
        {
            return _adapter.GetSchema().FindTable(tableName).GetDetail(relatedTableName);
        }

        private IDictionary<string, object> GetMaster(IDictionary<string, object> row, TableJoin masterJoin)
        {
            var criteria = new Dictionary<string, object> { { masterJoin.MasterColumn.ActualName, row[masterJoin.DetailColumn.HomogenizedName] } };
            return _adapter.Find(masterJoin.Master.ActualName,
                                       ExpressionHelper.CriteriaDictionaryToExpression(masterJoin.Master.ActualName,
                                                                                       criteria)).FirstOrDefault();
        }

        private SimpleQuery GetDetail(IDictionary<string, object> row, TableJoin join)
        {
            var criteria = ExpressionHelper.CriteriaDictionaryToExpression(join.Detail.ActualName,
                new Dictionary<string, object>
                               {{join.DetailColumn.ActualName, row[join.MasterColumn.HomogenizedName]}});
            return new SimpleQuery(_adapter, join.Detail.ActualName).Where(criteria);
        }
    }
}
