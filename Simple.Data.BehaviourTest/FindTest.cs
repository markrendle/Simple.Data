namespace Simple.Data.IntegrationTest
{
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class FindTest : DatabaseIntegrationContext
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
        public void TestFindWithNull()
        {
            _db.Users.Find(_db.Users.Id == null);
            GeneratedSqlIs("select [dbo].[users].* from [dbo].[users] where [dbo].[users].[id] IS NULL");
        }

        [Test]
        public void TestFindWithTwoCriteriasOneBeingNull()
        {
            _db.Users.Find(_db.Users.Id == 1 || _db.Users.Id == null);
            GeneratedSqlIs("select [dbo].[users].* from [dbo].[users] where ([dbo].[users].[id] = @p1 OR [dbo].[users].[id] IS NULL)");
            Parameter(0).Is(1);
            Parameter(1).DoesNotExist();
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
        public void TestFindModulo()
        {
            _db.Users.Find(_db.Users.Id % 2 == 1);
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] % 2 = @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindWithAdd()
        {
            _db.Users.Find(_db.Users.Id + _db.Users.Age == 42);
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] + [dbo].[Users].[Age] = @p1");
            Parameter(0).Is(42);
        }

        [Test]
        public void TestFindWithSubtract()
        {
            _db.Users.Find(_db.Users.Id - _db.Users.Age == 42);
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] - [dbo].[Users].[Age] = @p1");
            Parameter(0).Is(42);
        }

        [Test]
        public void TestFindWithMultiply()
        {
            _db.Users.Find(_db.Users.Id * _db.Users.Age == 42);
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] * [dbo].[Users].[Age] = @p1");
            Parameter(0).Is(42);
        }

        [Test]
        public void TestFindWithDivide()
        {
            _db.Users.Find(_db.Users.Id / _db.Users.Age == 42);
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[Id] / [dbo].[Users].[Age] = @p1");
            Parameter(0).Is(42);
        }
        
        [Test]
        public void TestFindByNamedParameterSingleColumn()
        {
            _db.Users.FindBy(Name: "Foo");
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where [dbo].[Users].[name] = @p1");
            Parameter(0).Is("Foo");
        }

        [Test]
        public void TestFindByNamedParameterTwoColumns()
        {
            _db.Users.FindBy(Name: "Foo", Password: "password");
            GeneratedSqlIs("select [dbo].[Users].* from [dbo].[Users] where ([dbo].[Users].[name] = @p1 and [dbo].[Users].[password] = @p2)");
            Parameter(0).Is("Foo");
            Parameter(1).Is("password");
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
        public void TestFindAllByNamedParameterSingleColumn()
        {
            _db.Users.FindAllBy(Name: "Foo").ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where [dbo].[Users].[name] = @p1");
            Parameter(0).Is("Foo");
        }

        [Test]
        public void TestFindAllByNamedParameterTwoColumns()
        {
            _db.Users.FindAllBy(Name: "Foo", Password: "password").ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where ([dbo].[Users].[name] = @p1 and [dbo].[Users].[password] = @p2)");
            Parameter(0).Is("Foo");
            Parameter(1).Is("password");
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
    }
}