using System;
using System.Data;
using Simple.Data.Ado;

namespace Simple.Data.Mocking.Ado
{
    public class MockConnectionProvider : IConnectionProvider
    {
        private readonly IDbConnection _connection;

        public MockConnectionProvider() : this(new MockDbConnection())
        {
        }

        public MockConnectionProvider(IDbConnection connection)
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
