using Simple.Data.Operations;

namespace Simple.Data
{
    using System.Collections.Generic;
    using System.Linq;

    internal class TransactionRunner : RunStrategy
    {
        private readonly IAdapterWithTransactions _adapter;
        private readonly IAdapterTransaction _adapterTransaction;

        public TransactionRunner(IAdapterWithTransactions adapter, IAdapterTransaction adapterTransaction)
        {
            _adapter = adapter;
            _adapterTransaction = adapterTransaction;
        }

        protected override Adapter Adapter
        {
            get { return (Adapter) _adapter; }
        }

        internal override IReadOnlyDictionary<string, object> FindOne(FindOperation operation)
        {
            return Find(operation).FirstOrDefault();
        }

        internal override int UpdateMany(string tableName, IList<IReadOnlyDictionary<string, object>> dataList)
        {
            return _adapter.UpdateMany(tableName, dataList, _adapterTransaction);
        }

        internal override int UpdateMany(string tableName, IList<IReadOnlyDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames)
        {
            return _adapter.UpdateMany(tableName, dataList, criteriaFieldNames, _adapterTransaction);
        }

        internal override int UpdateMany(string tableName, IList<IReadOnlyDictionary<string, object>> newValuesList, IList<IReadOnlyDictionary<string, object>> originalValuesList)
        {
            throw new System.NotImplementedException();
        }

        internal override IEnumerable<IReadOnlyDictionary<string, object>> Find(FindOperation operation)
        {
            return _adapter.Find(operation, _adapterTransaction);
        }

        internal override IEnumerable<IReadOnlyDictionary<string, object>> Insert(InsertOperation operation)
        {
            return _adapter.Insert(operation, _adapterTransaction);
        }

        internal override int Update(string tableName, IReadOnlyDictionary<string, object> data, SimpleExpression criteria)
        {
            return _adapter.Update(tableName, data, criteria, _adapterTransaction);
        }

        public override IEnumerable<IReadOnlyDictionary<string, object>> Upsert(UpsertOperation operation)
        {
            return _adapter.Upsert(operation, _adapterTransaction);
        }

        public override IReadOnlyDictionary<string, object> Get(GetOperation operation)
        {
            return _adapter.Get(operation, _adapterTransaction);
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return _adapter.RunQuery(query, _adapterTransaction, out unhandledClauses);
        }

        internal override int Update(string tableName, IReadOnlyDictionary<string, object> newValuesDict, IReadOnlyDictionary<string, object> originalValuesDict)
        {
            SimpleExpression criteria = CreateCriteriaFromOriginalValues(tableName, newValuesDict, originalValuesDict);
            var changedValuesDict = CreateChangedValuesDict(newValuesDict, originalValuesDict);
            return _adapter.Update(tableName, changedValuesDict, criteria, _adapterTransaction);
        }

        internal override int Delete(string tableName, SimpleExpression criteria)
        {
            return _adapter.Delete(tableName, criteria, _adapterTransaction);
        }
    }
}