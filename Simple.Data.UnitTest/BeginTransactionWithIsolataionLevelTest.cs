using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Simple.Data.Operations;

namespace Simple.Data.UnitTest
{
    [TestFixture]
    class BeginTransactionWithIsolataionLevelTest
    {
        [Test]
        public void TransactionsGetUnspecifiedIsolationLevelByDefault()
        {
            var adapter = new StubAdapterWithTransaction();
            Database db = new Database(adapter);
            db.BeginTransaction();

            Assert.AreEqual(IsolationLevel.Unspecified, adapter.IsolationLevel);
        }

        [Test]
        public void TransactionsGetExplicitlySetIsolationLevel()
        {
            var adapter = new StubAdapterWithTransaction();
            Database db = new Database(adapter);
            db.BeginTransaction(IsolationLevel.Serializable);

            Assert.AreEqual(IsolationLevel.Serializable, adapter.IsolationLevel);
        }

        [Test]
        public void NamedTransactionsGetUnspecifiedIsolationLevel()
        {
            var adapter = new StubAdapterWithTransaction();
            Database db = new Database(adapter);
            db.BeginTransaction("tran name");

            Assert.AreEqual(IsolationLevel.Unspecified, adapter.IsolationLevel);
        }
    }

    class StubAdapterWithTransaction : StubAdapter, IAdapterWithTransactions
    {
        public string TransactionName;
        public IsolationLevel IsolationLevel;

        public IAdapterTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            this.IsolationLevel = isolationLevel;
            return null;
        }

        public IAdapterTransaction BeginTransaction(string name, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            this.IsolationLevel = isolationLevel;
            this.TransactionName = name;
            return null;
        }

        #region IAdapterWithTransactions - not implementead

        public IEnumerable<IDictionary<string, object>> Find(FindOperation operation, IAdapterTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, IAdapterTransaction transaction, bool resultRequired)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDictionary<string, object>> InsertMany(string tableName, IEnumerable<IDictionary<string, object>> data, IAdapterTransaction transaction, Func<IDictionary<string, object>, Exception, bool> onError, bool resultRequired)
        {
            throw new NotImplementedException();
        }

        public int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public int Delete(string tableName, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IAdapterTransaction adapterTransaction)
        {
            throw new NotImplementedException();
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IAdapterTransaction adapterTransaction, IList<string> keyFields)
        {
            throw new NotImplementedException();
        }

        public int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames, IAdapterTransaction adapterTransaction)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> Get(GetOperation operation, IAdapterTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, IAdapterTransaction transaction, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}