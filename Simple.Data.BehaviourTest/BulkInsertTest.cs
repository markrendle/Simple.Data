namespace Simple.Data.IntegrationTest
{
    using System;
    using System.Dynamic;
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class BulkInsertTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" },
                                     new[] { "dbo", "NoIdentityColumnUsers", "BASE TABLE" });
            schemaProvider.SetColumns(new object[] { "dbo", "Users", "Id", true },
                                      new[] { "dbo", "Users", "Name" },
                                      new[] { "dbo", "Users", "Password" },
                                      new[] { "dbo", "Users", "Age" },
                                      new[] { "dbo", "NoIdentityColumnUsers", "Id" },
                                      new[] { "dbo", "NoIdentityColumnUsers", "Name" },
                                      new[] { "dbo", "NoIdentityColumnUsers", "Password" },
                                      new[] { "dbo", "NoIdentityColumnUsers", "Age" });
            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Users", "Id", 0 });
        }

        [Test]
        public void TestBulkInsertWithStaticTypeObjectAndIdentityColumn()
        {
            var users = new[] { new User { Name = "Steve", Age = 50 },  new User { Name = "Phil", Age = 42 }};
            _db.Users.Insert(users);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2)");
            Parameter(0).Is("Phil");
            Parameter(1).Is(DBNull.Value);
            Parameter(2).Is(42);
        }

        [Test]
        public void TestBulkInsertWithStaticTypeObjectAndIdentityColumnAndIdentityFunctionThatExpectsAValueSelects()
        {
            _MockConnectionProvider.SetIdentityFunction("@@IDENTITY");
            var users = new[] { new User { Name = "Steve", Age = 50 },  new User { Name = "Phil", Age = 42 }};
            var inserted = _db.Users.Insert(users);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2); select * from [dbo].[users] where [id] = @@identity");
            Parameter(0).Is("Phil");
            Parameter(1).Is(DBNull.Value);
            Parameter(2).Is(42);
            _MockConnectionProvider.SetIdentityFunction(null);
        }

        [Test]
        public void TestBulkInsertWithStaticTypeObjectAndIdentityColumnAndIdentityFunctionThatDoesNotExpectAValueDoesNotSelect()
        {
            _MockConnectionProvider.SetIdentityFunction("@@IDENTITY");
            var users = new[] { new User { Name = "Steve", Age = 50 },  new User { Name = "Phil", Age = 42 }};
            _db.Users.Insert(users);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2)");
            Parameter(0).Is("Phil");
            Parameter(1).Is(DBNull.Value);
            Parameter(2).Is(42);
            _MockConnectionProvider.SetIdentityFunction(null);
        }

        [Test]
        public void TestBulkInsertWithDynamicObjectAndIdentityColumn()
        {
            dynamic steve = new ExpandoObject();
            steve.Name = "Steve";
            steve.Age = 50;

            dynamic phil = new ExpandoObject();
            phil.Name = "Phil";
            phil.Age = 42;

            _db.Users.Insert(new[] {steve,phil});
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2)");
            Parameter(0).Is("Phil");
            Parameter(1).Is(DBNull.Value);
            Parameter(2).Is(42);
        }

        [Test]
        public void TestBulkInsertWithStaticTypeObjectAndNoIdentityColumn()
        {
            var users = new[] { new User { Id = 1, Name = "Steve", Age = 50 }, new User { Id = 2, Name = "Phil", Age = 42 } };
            _db.NoIdentityColumnUsers.Insert(users);
            GeneratedSqlIs("insert into [dbo].[NoIdentityColumnUsers] ([Id],[Name],[Password],[Age]) values (@p0,@p1,@p2,@p3)");
            Parameter(0).Is(2);
            Parameter(1).Is("Phil");
            Parameter(2).Is(DBNull.Value);
            Parameter(3).Is(42);
        }

        [Test]
        public void TestBulkInsertWithDynamicObjectAndNoIdentityColumn()
        {
            dynamic steve = new ExpandoObject();
            steve.Id = 1;
            steve.Name = "Steve";
            steve.Age = 50;

            dynamic phil = new ExpandoObject();
            phil.Id = 2;
            phil.Name = "Phil";
            phil.Age = 42;

            _db.NoIdentityColumnUsers.Insert(new[] { steve, phil });
            GeneratedSqlIs("insert into [dbo].[NoIdentityColumnUsers] ([Id],[Name],[Password],[Age]) values (@p0,@p1,@p2,@p3)");
            Parameter(0).Is(2);
            Parameter(1).Is("Phil");
            Parameter(2).Is(DBNull.Value);
            Parameter(3).Is(42);
        }

        [Test]
        // ReSharper disable InconsistentNaming
        public void TestThatInsertUsesDBNull()
            // ReSharper restore InconsistentNaming
        {
            dynamic person = new ExpandoObject();
            person.Name = null;
            _db.Users.Insert(person);
            Parameter(0).Is(DBNull.Value);
        }
    }
}