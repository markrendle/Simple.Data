namespace Simple.Data.IntegrationTest
{
    using System;
    using System.Collections.Generic;
    using Mocking.Ado;
    using Xunit;

    public class FindTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
// ReSharper disable CoVariantArrayConversion
            schemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" },
                                     new[] { "dbo", "MyTable", "BASE TABLE" });
            schemaProvider.SetColumns(new object[] { "dbo", "Users", "Id", true },
                                      new[] { "dbo", "Users", "Name" },
                                      new[] { "dbo", "Users", "Password" },
                                      new[] { "dbo", "Users", "Age" },
                                      new[] { "dbo", "MyTable", "Column1"});
            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Users", "Id", 0 });
// ReSharper restore CoVariantArrayConversion
        }

        private const string UsersColumns = "[dbo].[Users].[Id],[dbo].[Users].[Name],[dbo].[Users].[Password],[dbo].[Users].[Age]";
        [Fact]
        public async void TestFindEqualWithInt32()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id == 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[users] where [dbo].[users].[id] = @p1");
            Parameter(0).Is(1);
        }

        [Fact]
        public async void TestFindWithNull()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id == null);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[users] where [dbo].[users].[id] IS NULL");
        }

        [Fact]
        public async void TestFindWithTwoCriteriasOneBeingNull()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id == 1 || TargetDb.Users.Id == null);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[users] where ([dbo].[users].[id] = @p1 OR [dbo].[users].[id] IS NULL)");
            Parameter(0).Is(1);
            Parameter(1).DoesNotExist();
        }

        [Fact]
        public async void TestFindNotEqualWithInt32()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id != 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] != @p1");
            Parameter(0).Is(1);
        }

        [Fact]
        public async void TestFindGreaterThanWithInt32()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id > 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] > @p1");
            Parameter(0).Is(1);
        }

        [Fact]
        public async void TestFindGreaterThanOrEqualWithInt32()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id >= 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] >= @p1");
            Parameter(0).Is(1);
        }

        [Fact]
        public async void TestFindLessThanWithInt32()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id < 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] < @p1");
            Parameter(0).Is(1);
        }

        [Fact]
        public async void TestFindLessThanOrEqualWithInt32()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id <= 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] <= @p1");
            Parameter(0).Is(1);
        }

        [Fact]
        public async void TestFindModulo()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id % 2 == 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where ([dbo].[Users].[Id] % @p1) = @p2");
            Parameter(0).Is(2);
            Parameter(1).Is(1);
        }

        [Fact]
        public async void TestFindWithAdd()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id + TargetDb.Users.Age == 42);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where ([dbo].[Users].[Id] + [dbo].[Users].[Age]) = @p1");
            Parameter(0).Is(42);
        }

        [Fact]
        public async void TestFindWithAddAndMultiply()
        {
            await TargetDb.Users.Find((TargetDb.Users.Id + TargetDb.Users.Age) * 2 == 42);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where (([dbo].[Users].[Id] + [dbo].[Users].[Age]) * @p1) = @p2");
            Parameter(0).Is(2);
            Parameter(1).Is(42);
        }

        [Fact]
        public async void TestFindWithSubtract()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id - TargetDb.Users.Age == 42);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where ([dbo].[Users].[Id] - [dbo].[Users].[Age]) = @p1");
            Parameter(0).Is(42);
        }

        [Fact]
        public async void TestFindWithMultiply()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id * TargetDb.Users.Age == 42);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where ([dbo].[Users].[Id] * [dbo].[Users].[Age]) = @p1");
            Parameter(0).Is(42);
        }

        [Fact]
        public async void TestFindWithDivide()
        {
            await TargetDb.Users.Find(TargetDb.Users.Id / TargetDb.Users.Age == 42);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where ([dbo].[Users].[Id] / [dbo].[Users].[Age]) = @p1");
            Parameter(0).Is(42);
        }
        
        //[Fact]
        public async void TestFindByNamedParameterSingleColumn()
        {
            await TargetDb.Users.FindBy(Name: "Foo");
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[name] = @p1");
            Parameter(0).Is("Foo");
        }
        
        //[Fact]
        public async void TestFindByNamedParameterSingleColumnNullValue()
        {
            await TargetDb.Users.FindBy(Name: null);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[name] is null");
        }

        //[Fact]
        public async void TestFindByNamedParameterTwoColumns()
        {
            await TargetDb.Users.FindBy(Name: "Foo", Password: "password");
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where ([dbo].[Users].[name] = @p1 and [dbo].[Users].[password] = @p2)");
            Parameter(0).Is("Foo");
            Parameter(1).Is("password");
        }

        [Fact]
        public async void TestFindByDynamicSingleColumn()
        {
            var user = await TargetDb.Users.FindByName("Foo");
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[name] = @p1");
            Parameter(0).Is("Foo");
        }

        [Fact]
        public async void TestFindByDynamicSingleColumnNull()
        {
            await TargetDb.MyTable.FindByColumn1(null);
            GeneratedSqlIs("select [dbo].[MyTable].[Column1] from [dbo].[MyTable] where [dbo].[MyTable].[Column1] is null");
        }

        [Fact]
        public async void TestFindAllByDynamicSingleColumnNull()
        {
#pragma warning disable 168
            IEnumerable<MyTable> result = await TargetDb.MyTable.FindAllByColumn1(null).ToList<MyTable>();
#pragma warning restore 168
            GeneratedSqlIs("select [dbo].[MyTable].[Column1] from [dbo].[MyTable] where [dbo].[MyTable].[Column1] is null");
        }

        [Fact]
        public async void TestFindByWithAnonymousObject()
        {
            await TargetDb.MyTable.FindBy(new { Column1 = 1 });
            GeneratedSqlIs("select [dbo].[MyTable].[Column1] from [dbo].[MyTable] where [dbo].[MyTable].[Column1] = @p1");
            Parameter(0).Is(1);
        }

        [Fact]
        public async void TestFindByWithAnonymousObjectNullValue()
        {
            await TargetDb.MyTable.FindBy(new { Column1 = (object)null });
            GeneratedSqlIs("select [dbo].[MyTable].[Column1] from [dbo].[MyTable] where [dbo].[MyTable].[Column1] is null");
        }

        [Fact]
        public async void TestFindWithLike()
        {
            await TargetDb.Users.Find(TargetDb.Users.Name.Like("Foo"));
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[name] like @p1");
            Parameter(0).Is("Foo");
        }

        [Fact]
        public async void TestFindWithNotLike()
        {
            await TargetDb.Users.Find(TargetDb.Users.Name.NotLike("Foo"));
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[name] not like @p1");
            Parameter(0).Is("Foo");
        }

        [Fact]
        public void TestFindAllWithLike()
        {
            TargetDb.Users.FindAll(TargetDb.Users.Name.Like("Foo")).ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where [dbo].[Users].[name] like @p1");
            Parameter(0).Is("Foo");
        }

        [Fact]
        public void FindAllWithNoParametersThrowsBadExpressionException()
        {
            Assert.Throws<BadExpressionException>(() => TargetDb.Users.FindAll().ToList());
        }

        [Fact]
        public void FindAllWithStringParameterThrowsBadExpressionException()
        {
            Assert.Throws<BadExpressionException>(() => TargetDb.Users.FindAll("Answer").ToList());
        }

        [Fact]
        public void TestFindAllWithNotLike()
        {
            TargetDb.Users.FindAll(TargetDb.Users.Name.NotLike("Foo")).ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where [dbo].[Users].[name] not like @p1");
            Parameter(0).Is("Foo");
        }

        [Fact]
        public void TestFindAllByNamedParameterSingleColumn()
        {
            TargetDb.Users.FindAllBy(Name: "Foo").ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where [dbo].[Users].[name] = @p1");
            Parameter(0).Is("Foo");
        }

        [Fact]
        public void TestFindAllByNamedParameterSingleColumnNull()
        {
            TargetDb.Users.FindAllBy(Name: null).ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where [dbo].[Users].[name] is null");
        }

        [Fact]
        public void TestFindAllByNamedParameterTwoColumns()
        {
            TargetDb.Users.FindAllBy(Name: "Foo", Password: "password").ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where ([dbo].[Users].[name] = @p1 and [dbo].[Users].[password] = @p2)");
            Parameter(0).Is("Foo");
            Parameter(1).Is("password");
        }


        [Fact]
        public async void TestFindByDynamicTwoColumns()
        {
            await TargetDb.Users.FindByNameAndPassword("Foo", "secret");
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where ([dbo].[Users].[name] = @p1 and [dbo].[Users].[password] = @p2)");
            Parameter(0).Is("Foo");
            Parameter(1).Is("secret");
        }

        [Fact]
        public async void TestFindAllByDynamic()
        {
            await TargetDb.Users.FindAllByName("Foo").ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where [dbo].[Users].[name] = @p1");
            Parameter(0).Is("Foo");
        }

        [Fact]
        public async void FindByWithoutArgsThrowsArgumentException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => TargetDb.Users.FindBy());
        }
 
        [Fact]
        public void FindAllByWithoutArgsThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => TargetDb.Users.FindAllBy());
        }
    }

    class MyTable
    {
        public string Column1 { get; set; }
    }
}