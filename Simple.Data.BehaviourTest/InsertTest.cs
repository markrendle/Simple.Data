namespace Shitty.Data.IntegrationTest
{
    using System;
    using System.Dynamic;
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
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

        [Test]
        public void TestInsertWithNamedArguments()
        {
            _db.Users.Insert(Name: "Steve", Age: 50);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Age]) values (@p0,@p1)");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
        }

        [Test]
        public void TestInsertWithNamedArgumentsAndIdentityFunctionSelects()
        {
            _MockConnectionProvider.SetIdentityFunction("@@IDENTITY");
            var u = _db.Users.Insert(Name: "Steve", Age: 50);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Age]) values (@p0,@p1); select * from [dbo].[users] where [id] = @@identity");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
            _MockConnectionProvider.SetIdentityFunction(null);
        }

        [Test]
        public void TestInsertWithNamedArgumentsAndIdentityFunctionThatExpectsNoReturnValueDoesNotSelect()
        {
            _MockConnectionProvider.SetIdentityFunction("@@IDENTITY");
            _db.Users.Insert(Name: "Steve", Age: 50);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Age]) values (@p0,@p1)");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
            _MockConnectionProvider.SetIdentityFunction(null);
        }

        [Test]
        public void TestInsertWithStaticTypeObject()
        {
            var user = new User { Name = "Steve", Age = 50 };
            _db.Users.Insert(user);
            GeneratedSqlIs("insert into [dbo].[Users] ([Name],[Password],[Age]) values (@p0,@p1,@p2)");
            Parameter(0).Is("Steve");
            Parameter(1).Is(DBNull.Value);
            Parameter(2).Is(50);
        }

        [Test]
        public void TestInsertWithDynamicObject()
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