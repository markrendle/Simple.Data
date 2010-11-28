using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestFixture]
    public class DatabaseTest
    {
        static Database CreateDatabase(MockDatabase mockDatabase)
        {
            var mockSchemaProvider = new MockSchemaProvider();
            mockSchemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" });
            mockSchemaProvider.SetColumns(new[] { "dbo", "Users", "Id" },
                                          new[] { "dbo", "Users", "Name" },
                                          new[] { "dbo", "Users", "Password" },
                                          new[] { "dbo", "Users", "Age" });
            mockSchemaProvider.SetPrimaryKeys(new object[] {"dbo", "Users", "Id", 0});
            return new Database(new AdoAdapter(new MockConnectionProvider(new MockDbConnection(mockDatabase), mockSchemaProvider)));
        }

        [Test]
        public void TestFindEqualWithInt32()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.Find(database.Users.Id == 1);
            Assert.AreEqual("select [dbo].[users].* from [dbo].[users] where [dbo].[users].[id] = @p1".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, mockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindNotEqualWithInt32()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.Find(database.Users.Id != 1);
            Assert.AreEqual("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] != @p1".ToLowerInvariant().ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, mockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindGreaterThanWithInt32()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.Find(database.Users.Id > 1);
            Assert.AreEqual("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] > @p1".ToLowerInvariant().ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, mockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindGreaterThanOrEqualWithInt32()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.Find(database.Users.Id >= 1);
            Assert.AreEqual("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] >= @p1".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, mockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindLessThanWithInt32()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.Find(database.Users.Id < 1);
            Assert.AreEqual("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] < @p1".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, mockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindLessThanOrEqualWithInt32()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.Find(database.Users.Id <= 1);
            Assert.AreEqual("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] <= @p1".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, mockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindByDynamicSingleColumn()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.FindByName("Foo");
            Assert.AreEqual("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[name] like @p1".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Foo", mockDatabase.Parameters[0]);
        }

        [Test]
        public void TestFindByDynamicTwoColumns()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.FindByNameAndPassword("Foo", "secret");
            Assert.AreEqual("select [dbo].[Users].* from [dbo].[Users] where ([dbo].[Users].[name] like @p1 and [dbo].[Users].[password] like @p2)".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Foo", mockDatabase.Parameters[0]);
            Assert.AreEqual("secret", mockDatabase.Parameters[1]);
        }

        [Test]
        [Ignore]
        public void TestFindAllByDynamic()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.FindAllByName("Foo");
            Assert.AreEqual("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[name] like @p1".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Foo", mockDatabase.Parameters[0]);
        }

        [Test]
        public void TestInsertWithNamedArguments()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.Insert(Name: "Steve", Age: 50);
            Assert.AreEqual("insert into [dbo].[Users] ([Name],[Age]) values (@p0,@p1)".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Steve", mockDatabase.Parameters[0]);
            Assert.AreEqual(50, mockDatabase.Parameters[1]);
        }

        [Test]
        public void TestUpdateWithNamedArguments()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.UpdateById(Id: 1, Name: "Steve", Age: 50);
            Assert.AreEqual("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Steve", mockDatabase.Parameters[0]);
            Assert.AreEqual(50, mockDatabase.Parameters[1]);
            Assert.AreEqual(1, mockDatabase.Parameters[2]);
        }

        [Test]
        public void TestUpdateWithDynamicObject()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            dynamic record = new DynamicRecord();
            record.Id = 1;
            record.Name = "Steve";
            record.Age = 50;
            database.Users.Update(record);
            Assert.AreEqual("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Steve", mockDatabase.Parameters[0]);
            Assert.AreEqual(50, mockDatabase.Parameters[1]);
            Assert.AreEqual(1, mockDatabase.Parameters[2]);
        }

        [Test]
        public void TestUpdateByWithDynamicObject()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            dynamic record = new DynamicRecord();
            record.Id = 1;
            record.Name = "Steve";
            record.Age = 50;
            database.Users.UpdateById(record);
            Assert.AreEqual("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Steve", mockDatabase.Parameters[0]);
            Assert.AreEqual(50, mockDatabase.Parameters[1]);
            Assert.AreEqual(1, mockDatabase.Parameters[2]);
        }

        [Test]
        public void TestUpdateWithStaticObject()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            var user = new User
                           {
                               Id = 1,
                               Name = "Steve",
                               Age = 50
                           };
            database.Users.Update(user);
            Assert.AreEqual("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Steve", mockDatabase.Parameters[0]);
            Assert.AreEqual(50, mockDatabase.Parameters[1]);
            Assert.AreEqual(1, mockDatabase.Parameters[2]);
        }

        [Test]
        public void TestUpdateByWithStaticObject()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            var user = new User
                           {
                               Id = 1,
                               Name = "Steve",
                               Age = 50
                           };
            database.Users.UpdateById(user);
            Assert.AreEqual("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Steve", mockDatabase.Parameters[0]);
            Assert.AreEqual(50, mockDatabase.Parameters[1]);
            Assert.AreEqual(1, mockDatabase.Parameters[2]);
        }

        [Test]
        public void TestDeleteWithNamedArguments()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.Delete(Id: 1);
            Assert.AreEqual("delete from [dbo].[Users] where [dbo].[Users].[Id] = @p1".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, mockDatabase.Parameters[0]);
        }

        [Test]
        public void TestDeleteBy()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.DeleteById(1);
            Assert.AreEqual("delete from [dbo].[Users] where [dbo].[Users].[Id] = @p1".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, mockDatabase.Parameters[0]);
        }

        [Test]
        public void TestInsertOnTable()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            dynamic person = new ExpandoObject();
            person.Name = "Phil";
            person.Age = 42;
            database.Users.Insert(person);
            Assert.AreEqual("insert into [dbo].[Users] ([Name],[Age]) values (@p0,@p1)".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual("Phil", mockDatabase.Parameters[0]);
            Assert.AreEqual(42, mockDatabase.Parameters[1]);
        }
    }
}
