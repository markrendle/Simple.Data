namespace Simple.Data.IntegrationTest
{
    using System;
    using System.Dynamic;
    using Mocking.Ado;
    using Xunit;

    public class BulkInsertTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" },
                                     new[] { "dbo", "NoIdentityColumnUsers", "BASE TABLE" },
                                     new[] { "foo", "Users", "BASE TABLE" });
            schemaProvider.SetColumns(new object[] { "dbo", "Users", "Id", true },
                                      new[] { "dbo", "Users", "Name" },
                                      new[] { "dbo", "Users", "Password" },
                                      new[] { "dbo", "Users", "Age" },
                                      new object[] { "foo", "Users", "Id", true },
                                      new[] { "foo", "Users", "Name" },
                                      new[] { "foo", "Users", "Password" },
                                      new[] { "foo", "Users", "Age" },
                                      new[] { "dbo", "NoIdentityColumnUsers", "Id" },
                                      new[] { "dbo", "NoIdentityColumnUsers", "Name" },
                                      new[] { "dbo", "NoIdentityColumnUsers", "Password" },
                                      new[] { "dbo", "NoIdentityColumnUsers", "Age" });
            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Users", "Id", 0 });
        }

        [Fact]
        public async void TestBulkInsertWithStaticTypeObjectAndIdentityColumn()
        {
            var users = new[] { new User { Name = "Steve", Age = 50 },  new User { Name = "Phil", Age = 42 }};
            await TargetDb.Users.Insert(users);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2)");
            Parameter(0).Is("Phil");
            Parameter(1).Is(DBNull.Value);
            Parameter(2).Is(42);
        }
 
        [Fact]
        public async void TestBulkInsertWithStaticTypeObjectAndIdentityColumnOnSchemaQualifiedTable()
        {
            var users = new[] { new User { Name = "Steve", Age = 50 },  new User { Name = "Phil", Age = 42 }};
            await TargetDb.foo.Users.Insert(users);
            GeneratedSqlIs("insert into [foo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2)");
            Parameter(0).Is("Phil");
            Parameter(1).Is(DBNull.Value);
            Parameter(2).Is(42);
        }

        [Fact]
        public async void TestBulkInsertWithStaticTypeObjectAndIdentityColumnAndIdentityFunctionThatExpectsAValueSelects()
        {
            _MockConnectionProvider.SetIdentityFunction("@@IDENTITY");
            var users = new[] { new User { Name = "Steve", Age = 50 },  new User { Name = "Phil", Age = 42 }};
            var inserted = await TargetDb.Users.Insert(users);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2); select * from [dbo].[users] where [id] = @@identity");
            Parameter(0).Is("Phil");
            Parameter(1).Is(DBNull.Value);
            Parameter(2).Is(42);
            _MockConnectionProvider.SetIdentityFunction(null);
        }

        //[Fact]
        public async void TestBulkInsertWithStaticTypeObjectAndIdentityColumnAndIdentityFunctionThatDoesNotExpectAValueDoesNotSelect()
        {
            _MockConnectionProvider.SetIdentityFunction("@@IDENTITY");
            var users = new[] { new User { Name = "Steve", Age = 50 },  new User { Name = "Phil", Age = 42 }};
            await TargetDb.Users.Insert(users);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2)");
            Parameter(0).Is("Phil");
            Parameter(1).Is(DBNull.Value);
            Parameter(2).Is(42);
            _MockConnectionProvider.SetIdentityFunction(null);
        }

        //[Fact]
        public async void TestBulkInsertWithDynamicObjectAndIdentityColumn()
        {
            dynamic steve = new ExpandoObject();
            steve.Name = "Steve";
            steve.Age = 50;

            dynamic phil = new ExpandoObject();
            phil.Name = "Phil";
            phil.Age = 42;

            await TargetDb.Users.Insert(new[] {steve,phil});
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2)");
            Parameter(0).Is("Phil");
            Parameter(1).Is(DBNull.Value);
            Parameter(2).Is(42);
        }

        [Fact]
        public async void TestBulkInsertWithStaticTypeObjectAndNoIdentityColumn()
        {
            var users = new[] { new User { Id = 1, Name = "Steve", Age = 50 }, new User { Id = 2, Name = "Phil", Age = 42 } };
            await TargetDb.NoIdentityColumnUsers.Insert(users);
            GeneratedSqlIs("insert into [dbo].[NoIdentityColumnUsers] ([Id],[Name],[Password],[Age]) values (@p0,@p1,@p2,@p3)");
            Parameter(0).Is(2);
            Parameter(1).Is("Phil");
            Parameter(2).Is(DBNull.Value);
            Parameter(3).Is(42);
        }

        [Fact]
        public async void TestBulkInsertWithDynamicObjectAndNoIdentityColumn()
        {
            dynamic steve = new ExpandoObject();
            steve.Id = 1;
            steve.Name = "Steve";
            steve.Age = 50;

            dynamic phil = new ExpandoObject();
            phil.Id = 2;
            phil.Name = "Phil";
            phil.Age = 42;

            await TargetDb.NoIdentityColumnUsers.Insert(new[] { steve, phil });
            GeneratedSqlIs("insert into [dbo].[NoIdentityColumnUsers] ([Id],[Name],[Password],[Age]) values (@p0,@p1,@p2,@p3)");
            Parameter(0).Is(2);
            Parameter(1).Is("Phil");
            Parameter(2).Is(DBNull.Value);
            Parameter(3).Is(42);
        }

        //[Fact]
        // ReSharper disable InconsistentNaming
        public async void TestThatInsertUsesDBNull()
            // ReSharper restore InconsistentNaming
        {
            dynamic person = new ExpandoObject();
            person.Name = null;
            await TargetDb.Users.Insert(person);
            Parameter(0).Is(DBNull.Value);
        }
    }
}