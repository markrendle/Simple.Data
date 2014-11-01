using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Simple.Data.Operations;

namespace Simple.Data.UnitTest
{
    using System.Threading.Tasks;

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

        public Task<OperationResult> Execute(IOperation operation, IAdapterTransaction transaction)
        {
            throw new NotImplementedException();
        }
    }

}