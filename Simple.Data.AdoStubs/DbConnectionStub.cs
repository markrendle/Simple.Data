using System;
using System.Data;

namespace Simple.Data.AdoStubs
{
    public class DbConnectionStub : IDbConnection
    {
        public DataTable DummyDataTable { get; set; }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            throw new NotImplementedException();
        }

        public IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            
        }

        public string ConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int ConnectionTimeout
        {
            get { throw new NotImplementedException(); }
        }

        public IDbCommand CreateCommand()
        {
            return new DbCommandStub(this);
        }

        public string Database
        {
            get { throw new NotImplementedException(); }
        }

        public void Open()
        {
            
        }

        public ConnectionState State
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
        }
    }
}
