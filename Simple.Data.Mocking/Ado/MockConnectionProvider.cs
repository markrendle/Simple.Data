using System;
using System.Data;
using System.Data.Common;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Mocking.Ado
{
    public class MockConnectionProvider : IConnectionProvider
    {
        private readonly DbConnection _connection;
        private readonly MockSchemaProvider _mockSchemaProvider;

        public MockConnectionProvider(DbConnection connection, MockSchemaProvider mockSchemaProvider)
        {
            _connection = connection;
            _mockSchemaProvider = mockSchemaProvider;
        }

        public void SetConnectionString(string connectionString)
        {
            throw new NotImplementedException();
        }

        public DbConnection CreateConnection()
        {
            return _connection;
        }

        public ISchemaProvider GetSchemaProvider()
        {
            return _mockSchemaProvider;
        }

        public string ConnectionString
        {
            get { return _connection.ConnectionString; }
        }
    }
}
