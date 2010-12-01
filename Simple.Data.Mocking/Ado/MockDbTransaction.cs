using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Simple.Data.Mocking.Ado
{
    class MockDbTransaction : DbTransaction
    {
        public static bool CommitCalled { get; private set; }
        public static bool RollbackCalled { get; private set; }
        private readonly DbConnection _dbConnection;
        private readonly IsolationLevel _isolationLevel;

        public MockDbTransaction(DbConnection dbConnection, IsolationLevel isolationLevel)
        {
            _dbConnection = dbConnection;
            _isolationLevel = isolationLevel;
        }

        public override void Commit()
        {
            CommitCalled = true;
        }

        public override void Rollback()
        {
            RollbackCalled = true;
        }

        protected override DbConnection DbConnection
        {
            get { return _dbConnection; }
        }

        public override IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
        }
    }
}
