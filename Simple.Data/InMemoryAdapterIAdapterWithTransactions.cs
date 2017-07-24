using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Shitty.Data
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

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            return Find(tableName, criteria);
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, IAdapterTransaction transaction, bool resultRequired)
        {
            return Insert(tableName, data, resultRequired);
        }

        public IEnumerable<IDictionary<string, object>> InsertMany(string tableName, IEnumerable<IDictionary<string, object>> data, IAdapterTransaction transaction, Func<IDictionary<string, object>, Exception, bool> onError, bool resultRequired)
        {
            return InsertMany(tableName, data, onError, resultRequired);
        }

        public int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            return Update(tableName, data, criteria);
        }

        public int Delete(string tableName, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            return Delete(tableName, criteria);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IAdapterTransaction adapterTransaction)
        {
            return UpdateMany(tableName, dataList);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IAdapterTransaction adapterTransaction, IList<string> keyFields)
        {
            return UpdateMany(tableName, dataList, keyFields);
        }

        public int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames, IAdapterTransaction adapterTransaction)
        {
            return UpdateMany(tableName, dataList, criteriaFieldNames);
        }

        public IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, IAdapterTransaction transaction, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return RunQuery(query, out unhandledClauses);
        }
    }
}
