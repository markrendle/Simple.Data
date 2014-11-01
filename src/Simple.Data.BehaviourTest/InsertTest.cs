namespace Simple.Data.IntegrationTest
{
    using System;
    using System.Dynamic;
    using Mocking.Ado;
    using Xunit;

    public class InsertTest: DatabaseIntegrationContext
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

        [Fact]
        public async void TestInsertWithNamedArguments()
        {
            await TargetDb.Users.Insert(Name: "Steve", Age: 50);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Age]) values (@p0,@p1)");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
        }

        [Fact]
        public async void TestInsertWithNamedArgumentsAndIdentityFunctionSelects()
        {
            _MockConnectionProvider.SetIdentityFunction("@@IDENTITY");
            var u = await TargetDb.Users.Insert(Name: "Steve", Age: 50);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Age]) values (@p0,@p1); select * from [dbo].[users] where [id] = @@identity");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
            _MockConnectionProvider.SetIdentityFunction(null);
        }

        [Fact]
        public async void TestInsertWithStaticTypeObject()
        {
            var user = new User { Name = "Steve", Age = 50 };
            await TargetDb.Users.Insert(user);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2)");
            Parameter(0).Is("Steve");
            Parameter(1).Is(DBNull.Value);
            Parameter(2).Is(50);
        }

        [Fact]
        public async void TestInsertWithDynamicObject()
        {
            dynamic person = new ExpandoObject();
            person.Name = "Phil";
            person.Age = 42;
            await TargetDb.Users.Insert(person);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Age]) values (@p0,@p1)");
            Parameter(0).Is("Phil");
            Parameter(1).Is(42);
        }

        [Fact]
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