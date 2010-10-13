using System;
using System.Data;
using Simple.Data.Ado;

namespace Simple.Data.AdoStubs
{
    public class ConnectionProviderStub : IConnectionProvider
    {
        private readonly IDbConnection _connection;

        public ConnectionProviderStub() : this(new DbConnectionStub())
        {
        }

        public ConnectionProviderStub(IDbConnection connection)
        {
            _connection = connection;
        }

        public void SetConnectionString(string connectionString)
        {
            throw new NotImplementedException();
        }

        public IDbConnection CreateConnection()
        {
            return _connection;
        }
    }
}
