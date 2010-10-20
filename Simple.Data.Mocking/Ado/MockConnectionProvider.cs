using System;
using System.Data;
using System.Data.Common;
using Simple.Data.Ado;

namespace Simple.Data.Mocking.Ado
{
    public class MockConnectionProvider : IConnectionProvider
    {
        private readonly DbConnection _connection;

        public MockConnectionProvider() : this(new MockDbConnection())
        {
        }

        public MockConnectionProvider(DbConnection connection)
        {
            _connection = connection;
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
            return new MockSchemaProvider();
        }

        public DataTable GetSchema(string collectionName)
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchema(string collectionName, params string[] restrictionValues)
        {
            throw new NotImplementedException();
        }
    }
}
