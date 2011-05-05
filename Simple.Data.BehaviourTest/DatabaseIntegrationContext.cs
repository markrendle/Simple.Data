using System;
using System.Dynamic;
using NUnit.Framework;
using Simple.Data.Mocking;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    public abstract class DatabaseIntegrationContext
    {
        private MockDatabase _mockDatabase;
        protected dynamic _db;

        protected abstract void SetSchema(MockSchemaProvider schemaProvider);

        [SetUp]
        public void Setup()
        {
            _db = GetOpenDataBase();
        }

        protected Database GetOpenDataBase()
        {
            _mockDatabase = new MockDatabase();
            return CreateDatabase(_mockDatabase);
        }

        protected void GeneratedSqlIs(string sql)
        {
            if (_mockDatabase.Sql == null)
                Assert.Fail("No SQL was generated");
            Assert.AreEqual(sql.ToLowerInvariant(), _mockDatabase.Sql.ToLowerInvariant());
        }

        protected dynamic Parameter(int index)
        {
            dynamic obj = new ExpandoObject();
            obj.Is = new Action<object>(v => Assert.AreEqual(v, _mockDatabase.Parameters[index]));
            obj.IsDBNull = new Action(() => Assert.AreEqual(DBNull.Value, _mockDatabase.Parameters[index]));
            return obj;
        }

        Database CreateDatabase(MockDatabase mockDatabase)
        {
            var mockSchemaProvider = new MockSchemaProvider();

            SetSchema(mockSchemaProvider);

            var adapter = MockHelper.CreateMockAdoAdapter(
                new MockConnectionProvider(new MockDbConnection(mockDatabase), mockSchemaProvider));
            MockHelper.UseMockAdapter(adapter);
            return Database.Open();
        }
    }
}