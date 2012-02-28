using System;
using System.Dynamic;
using NUnit.Framework;
using Simple.Data.Mocking;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    using System.Diagnostics;

    public abstract class DatabaseIntegrationContext
    {
        protected MockDatabase _mockDatabase;
        protected Mocking.Ado.MockConnectionProvider _MockConnectionProvider;
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
            obj.Exists = new Action(()=> Assert.DoesNotThrow(() => _mockDatabase.Parameters[index].ToString()));
            obj.DoesNotExist = new Action(
                    () => Assert.Throws<IndexOutOfRangeException>(() => _mockDatabase.Parameters[index].ToString()));
            return obj;
        }

        Database CreateDatabase(MockDatabase mockDatabase)
        {
            var mockSchemaProvider = new MockSchemaProvider();

            SetSchema(mockSchemaProvider);

            _MockConnectionProvider = new Mocking.Ado.MockConnectionProvider(new MockDbConnection(mockDatabase), mockSchemaProvider);
            var adapter = MockHelper.CreateMockAdoAdapter(_MockConnectionProvider);
            MockHelper.UseMockAdapter(adapter);
            return Database.Open();
        }

        protected static void EatException(Action action)
        {
            EatException<Exception>(action);
        }

        protected static void EatException<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
                Trace.TraceError(ex.Message);
            }

        }
    }
}