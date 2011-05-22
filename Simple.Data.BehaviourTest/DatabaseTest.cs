using System;
using System.Dynamic;
using NUnit.Framework;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
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
        public void TestFindEqualWithInt32()
        {
            _db.Users.Find(_db.Users.Id == 1);
            GeneratedSqlIs("select [dbo].[users].* from [dbo].[users] where [dbo].[users].[id] = @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindNotEqualWithInt32()
        {
            _db.Users.Find(_db.Users.Id != 1);
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] != @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindGreaterThanWithInt32()
        {
            _db.Users.Find(_db.Users.Id > 1);
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] > @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindGreaterThanOrEqualWithInt32()
        {
            _db.Users.Find(_db.Users.Id >= 1);
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] >= @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindLessThanWithInt32()
        {
            _db.Users.Find(_db.Users.Id < 1);
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] < @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindLessThanOrEqualWithInt32()
        {
            _db.Users.Find(_db.Users.Id <= 1);
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] <= @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindByDynamicSingleColumn()
        {
            _db.Users.FindByName("Foo");
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[name] = @p1");
            Parameter(0).Is("Foo");
        }

        [Test]
        public void TestFindWithLike()
        {
            _db.Users.Find(_db.Users.Name.Like("Foo"));
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[name] like @p1");
            Parameter(0).Is("Foo");
        }

        [Test]
        public void TestFindAllWithLike()
        {
            _db.Users.FindAll(_db.Users.Name.Like("Foo")).ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where [dbo].[Users].[name] like @p1");
            Parameter(0).Is("Foo");
        }

        [Test]
        public void TestFindByDynamicTwoColumns()
        {
            _db.Users.FindByNameAndPassword("Foo", "secret");
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where ([dbo].[Users].[name] = @p1 and [dbo].[Users].[password] = @p2)");
            Parameter(0).Is("Foo");
            Parameter(1).Is("secret");
        }

        [Test]
        public void TestFindAllByDynamic()
        {
            _db.Users.FindAllByName("Foo").ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where [dbo].[Users].[name] = @p1");
            Parameter(0).Is("Foo");
        }

        [Test]
        public void TestInsertWithNamedArguments()
        {
            _db.Users.Insert(Name: "Steve", Age: 50);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Age]) values (@p0,@p1)");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
        }

        [Test]
        public void TestInsertWithStaticTypeObject()
        {
            var user = new User { Name = "Steve", Age = 50 };
            _db.Users.Insert(user);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2)");
            Parameter(0).Is("Steve");
            Parameter(1).Is(null);
            Parameter(2).Is(50);
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

        [Test]
        public void TestInsertOnTable()
        {
            dynamic person = new ExpandoObject();
            person.Name = "Phil";
            person.Age = 42;
            _db.Users.Insert(person);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Age]) values (@p0,@p1)");
            Parameter(0).Is("Phil");
            Parameter(1).Is(42);
        }

        [Test]
        public void TestThatInsertUsesDBNull()
        {
            dynamic person = new ExpandoObject();
            person.Name = null;
            _db.Users.Insert(person);
            Parameter(0).Is(DBNull.Value);
        }
    }
}
