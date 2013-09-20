using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Operations;

namespace Simple.Data
{
    public partial class InMemoryAdapter : IAdapterWithTransactions
    {
        class InMemoryAdapterTransaction : IAdapterTransaction
        {
            private readonly string _name;

            public InMemoryAdapterTransaction() : this(string.Empty)
            {
            }

            public InMemoryAdapterTransaction(string name)
            {
                _name = name;
            }

            public void Dispose()
            {
            }

            public void Commit()
            {
            }

            public void Rollback()
            {
            }

            public string Name
            {
                get { return _name; }
            }
        }

        public IAdapterTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            return new InMemoryAdapterTransaction();
        }

        public IAdapterTransaction BeginTransaction(string name, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            return new InMemoryAdapterTransaction(name);
        }

        public IEnumerable<IReadOnlyDictionary<string, object>> Find(FindOperation operation, IAdapterTransaction transaction)
        {
            return Find(operation);
        }

        public IEnumerable<IReadOnlyDictionary<string, object>> Insert(InsertOperation operation, IAdapterTransaction transaction)
        {
            return Insert(operation);
        }

        public int Update(string tableName, IReadOnlyDictionary<string, object> data, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            return Update(tableName, data, criteria);
        }

        public int Delete(string tableName, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            return Delete(tableName, criteria);
        }

        public int UpdateMany(string tableName, IEnumerable<IReadOnlyDictionary<string, object>> dataList, IAdapterTransaction adapterTransaction)
        {
            return UpdateMany(tableName, dataList);
        }

        public int UpdateMany(string tableName, IEnumerable<IReadOnlyDictionary<string, object>> dataList, IAdapterTransaction adapterTransaction, IList<string> keyFields)
        {
            return UpdateMany(tableName, dataList, keyFields);
        }

        public int UpdateMany(string tableName, IList<IReadOnlyDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames, IAdapterTransaction adapterTransaction)
        {
            return UpdateMany(tableName, dataList, criteriaFieldNames);
        }

        public IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, IAdapterTransaction transaction, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return RunQuery(query, out unhandledClauses);
        }

        public IEnumerable<IReadOnlyDictionary<string, object>> Upsert(UpsertOperation operation,
            IAdapterTransaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}
