using NUnit.Framework;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    using System.Collections.Generic;

    [TestFixture]
    public class DatabaseTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" });
            schemaProvider.SetColumns(new object[] { "dbo", "Users", "Id", true },
                                      new[] { "dbo", "Users", "Name" },
                                      new[] { "dbo", "Users", "Password" },
                                      new[] { "dbo", "Users", "Age" });
            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Users", "Id", 0 });
        }

        [Test]
        public void TestUpdateWithNamedArguments()
        {
            _db.Users.UpdateById(Id: 1, Name: "Steve", Age: 50);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
            Parameter(2).Is(1);
        }

        [Test]
        public void TestUpdateWithDynamicObject()
        {
            dynamic record = new SimpleRecord();
            record.Id = 1;
            record.Name = "Steve";
            record.Age = 50;
            _db.Users.Update(record);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
            Parameter(2).Is(1);
        }

        [Test]
        public void TestUpdateWithDynamicObjectList()
        {
            dynamic record1 = new SimpleRecord();
            record1.Id = 1;
            record1.Name = "Steve";
            record1.Age = 50;
            dynamic record2 = new SimpleRecord();
            record2.Id = 2;
            record2.Name = "Bob";
            record2.Age = 42;
            _db.Users.Update(new[] { record1, record2});
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3");
            Parameter(0).Is("Bob");
            Parameter(1).Is(42);
            Parameter(2).Is(2);
        }

        [Test]
        public void TestUpdateByWithDynamicObjectList()
        {
            dynamic record1 = new SimpleRecord();
            record1.Id = 1;
            record1.Name = "Steve";
            record1.Age = 50;
            dynamic record2 = new SimpleRecord();
            record2.Id = 2;
            record2.Name = "Bob";
            record2.Age = 42;
            _db.Users.UpdateById(new[] { record1, record2 });
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3");
            Parameter(0).Is("Bob");
            Parameter(1).Is(42);
            Parameter(2).Is(2);
        }

        [Test]
        public void TestUpdateByWithDynamicObject()
        {
            dynamic record = new SimpleRecord();
            record.Id = 1;
            record.Name = "Steve";
            record.Age = 50;
            _db.Users.UpdateById(record);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
            Parameter(2).Is(1);
        }

        [Test]
        public void TestUpdateWithStaticObject()
        {
            var user = new User
                           {
                               Id = 1,
                               Name = "Steve",
                               Age = 50
                           };
            _db.Users.Update(user);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Password] = @p2, [Age] = @p3 where [dbo].[Users].[Id] = @p4");
            Parameter(0).Is("Steve");
            Parameter(1).IsDBNull();
            Parameter(2).Is(50);
            Parameter(3).Is(1);
        }

        [Test]
        public void TestUpdateWithStaticObjectList()
        {
            var users = new[]
                            {
                                new User { Id = 2, Name = "Bob", Age = 42 },
                                new User { Id = 1, Name = "Steve", Age = 50},
                            };
            _db.Users.Update(users);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Password] = @p2, [Age] = @p3 where [dbo].[Users].[Id] = @p4");
            Parameter(0).Is("Steve");
            Parameter(1).IsDBNull();
            Parameter(2).Is(50);
            Parameter(3).Is(1);
        }

        [Test]
        public void TestUpdateByWithStaticObjectList()
        {
            var users = new[]
                            {
                                new User { Id = 2, Name = "Bob", Age = 42 },
                                new User { Id = 1, Name = "Steve", Age = 50},
                            };
            _db.Users.UpdateById(users);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Password] = @p2, [Age] = @p3 where [dbo].[Users].[Id] = @p4");
            Parameter(0).Is("Steve");
            Parameter(1).IsDBNull();
            Parameter(2).Is(50);
            Parameter(3).Is(1);
        }

        [Test]
        public void TestUpdateByWithStaticObject()
        {
            var user = new User
                           {
                               Id = 1,
                               Name = "Steve",
                               Age = 50
                           };
            _db.Users.UpdateById(user);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Password] = @p2, [Age] = @p3 where [dbo].[Users].[Id] = @p4");
            Parameter(0).Is("Steve");
            Parameter(1).IsDBNull();
            Parameter(2).Is(50);
            Parameter(3).Is(1);
        }

        [Test]
        public void TestThatUpdateUsesDbNullForNullValues()
        {
            _db.Users.UpdateById(Id: 1, Name: null);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1 where [dbo].[Users].[Id] = @p2");
            Parameter(0).IsDBNull();
        }

        [Test]
        public void TestUpdateAll()
        {
            _db.Users.UpdateAll(Name: "Steve");
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1");
            Parameter(0).Is("Steve");
        }

        [Test]
        public void TestUpdateWithCriteria()
        {
            _db.Users.UpdateAll(_db.Users.Age > 30, Name: "Steve");
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1 where [dbo].[Users].[Age] > @p2");
            Parameter(0).Is("Steve");
            Parameter(1).Is(30);
        }

        [Test]
        public void TestUpdateWithCriteriaAndDictionary()
        {
            var data = new Dictionary<string, object> { { "Name", "Steve" } };
            _db.Users.UpdateAll(_db.Users.Age > 30, data);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1 where [dbo].[Users].[Age] > @p2");
            Parameter(0).Is("Steve");
            Parameter(1).Is(30);
        }

        [Test]
        public void TestUpdateWithCriteriaAsNamedArg()
        {
            _db.Users.UpdateAll(Name: "Steve", Condition: _db.Users.Age > 30);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1 where [dbo].[Users].[Age] > @p2");
            Parameter(0).Is("Steve");
            Parameter(1).Is(30);
        }

        [Test]
        public void TestDeleteWithNamedArguments()
        {
            _db.Users.Delete(Id: 1);
            GeneratedSqlIs("delete from [dbo].[Users] where [dbo].[Users].[Id] = @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestDeleteBy()
        {
            _db.Users.DeleteById(1);
            GeneratedSqlIs("delete from [dbo].[Users] where [dbo].[Users].[Id] = @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestDeleteAllWithNoCriteria()
        {
            _db.Users.DeleteAll();
            GeneratedSqlIs("delete from [dbo].[Users]");
        }

        [Test]
        public void TestDeleteAllWithSimpleCriteria()
        {
            _db.Users.DeleteAll(_db.Users.Age > 42);
            GeneratedSqlIs("delete from [dbo].[Users] where [dbo].[Users].[Age] > @p1");
            Parameter(0).Is(42);
        }

        [Test]
        public void TestDeleteAllMoreComplexCriteria()
        {
            _db.Users.DeleteAll(_db.Users.Age > 42 && _db.Users.Name.Like("J%"));
            GeneratedSqlIs("delete from [dbo].[Users] where ([dbo].[Users].[Age] > @p1 AND [dbo].[Users].[Name] LIKE @p2)");
            Parameter(0).Is(42);
            Parameter(1).Is("J%");
        }

    }
}
