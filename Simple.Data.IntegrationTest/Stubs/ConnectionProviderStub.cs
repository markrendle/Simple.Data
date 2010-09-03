using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Ado;

namespace Simple.Data.IntegrationTest.Stubs
{
    class ConnectionProviderStub : IConnectionProvider
    {
        private readonly IDbConnection _connection;

        public ConnectionProviderStub() : this(new DbConnectionStub())
        {
        }

        public ConnectionProviderStub(IDbConnection connection)
        {
            _connection = connection;
        }

        public IDbConnection CreateConnection()
        {
            return _connection;
        }
    }
}
