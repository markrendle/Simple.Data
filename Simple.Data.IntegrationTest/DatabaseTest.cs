using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestFixture]
    public class DatabaseTest
    {
        [ThreadStatic]
        private static dynamic _testDatabase;

        static dynamic TestDatabase
        {
            get { return _testDatabase ?? (_testDatabase = CreateDatabase()); }
        }

        static Database CreateDatabase()
        {
            MockSchemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" });
            MockSchemaProvider.SetColumns(new[] { "dbo", "Users", "Id" },
                                          new[] { "dbo", "Users", "Name" },
                                          new[] { "dbo", "Users", "Password" },
                                          new[] { "dbo", "Users", "Age" });
            return new Database(new MockConnectionProvider(new MockDbConnection()));
        }

        [Test]
        public void TestFindEqualWithInt32()
        {
            TestDatabase.Users.Find(TestDatabase.Users.Id == 1);
            Assert.AreEqual("select [users].* from [users] where [users].[id] = @p1".ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, MockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindNotEqualWithInt32()
        {
            TestDatabase.Users.Find(TestDatabase.Users.Id != 1);
            Assert.AreEqual("select [Users].* from [Users] where [Users].[Id] != @p1".ToLowerInvariant().ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, MockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindGreaterThanWithInt32()
        {
            TestDatabase.Users.Find(TestDatabase.Users.Id > 1);
            Assert.AreEqual("select [Users].* from [Users] where [Users].[Id] > @p1".ToLowerInvariant().ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, MockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindGreaterThanOrEqualWithInt32()
        {
            TestDatabase.Users.Find(TestDatabase.Users.Id >= 1);
            Assert.AreEqual("select [Users].* from [Users] where [Users].[Id] >= @p1".ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, MockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindLessThanWithInt32()
        {
            TestDatabase.Users.Find(TestDatabase.Users.Id < 1);
            Assert.AreEqual("select [Users].* from [Users] where [Users].[Id] < @p1".ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, MockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindLessThanOrEqualWithInt32()
        {
            TestDatabase.Users.Find(TestDatabase.Users.Id <= 1);
            Assert.AreEqual("select [Users].* from [Users] where [Users].[Id] <= @p1".ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, MockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindByDynamicSingleColumn()
        {
            TestDatabase.Users.FindByName("Foo");
            Assert.AreEqual("select [Users].* from [Users] where [Users].[name] like @p1".ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Foo", MockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindByDynamicTwoColumns()
        {
            TestDatabase.Users.FindByNameAndPassword("Foo", "secret");
            Assert.AreEqual("select [Users].* from [Users] where ([Users].[name] like @p1 and [Users].[password] like @p2)".ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Foo", MockDatabase.Parameters[0]);
            Assert.AreEqual("secret", MockDatabase.Parameters[1]);
        }

        [Test]
        public void TestFindAllByDynamic()
        {
            TestDatabase.Users.FindAllByName("Foo");
            Assert.AreEqual("select [Users].* from [Users] where [Users].[name] like @p1".ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Foo", MockDatabase.Parameters[0]);
        }

        [Test]
        public void TestInsertWithNamedArguments()
        {
            TestDatabase.Users.Insert(Name: "Steve", Age: 50);
            Assert.AreEqual("insert into [Users] ([Name],[Age]) values (@p0,@p1)".ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Steve", MockDatabase.Parameters[0]);
            Assert.AreEqual(50, MockDatabase.Parameters[1]);
        }

        [Test]
        public void TestUpdateWithNamedArguments()
        {
            TestDatabase.Users.UpdateById(Id: 1, Name: "Steve", Age: 50);
            Assert.AreEqual("update [Users] set [Name] = @p0, [Age] = @p1 where [Id] = @p2".ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Steve", MockDatabase.Parameters[0]);
            Assert.AreEqual(50, MockDatabase.Parameters[1]);
            Assert.AreEqual(1, MockDatabase.Parameters[2]);
        }

        [Test]
        public void TestDeleteWithNamedArguments()
        {
            TestDatabase.Users.Delete(Id: 1);
            Assert.AreEqual("delete from [Users] where [Id] = @p0".ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, MockDatabase.Parameters[0]);
        }

        [Test]
        public void TestInsertOnTable()
        {
            dynamic person = new ExpandoObject();
            person.Name = "Phil";
            person.Age = 42;
            TestDatabase.Users.Insert(person);
            Assert.AreEqual("insert into [Users] ([Name],[Age]) values (@p0,@p1)".ToLowerInvariant(), MockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Phil", MockDatabase.Parameters[0]);
            Assert.AreEqual(42, MockDatabase.Parameters[1]);
        }


        [Test]
        public void TestAllPropertyShouldWriteDeprecatedMessageToTrace()
        {
            var traceListener = new TestTraceListener();
            Trace.Listeners.Add(traceListener);

            var dummy = TestDatabase.Users.All;
            Assert.IsTrue(traceListener.Messages.Contains("deprecated"));

            Trace.Listeners.Remove(traceListener);
        }
    }
}
