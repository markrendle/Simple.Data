namespace Simple.Data.IntegrationTest
{
    using System;
    using System.Collections.Generic;
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
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

        private const string UsersColumns = "[dbo].[Users].[Id], [dbo].[Users].[Name], [dbo].[Users].[Password], [dbo].[Users].[Age]";
        [Test]
        public void TestFindEqualWithInt32()
        {
            _db.Users.Find(_db.Users.Id == 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[users] where [dbo].[users].[id] = @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindWithNull()
        {
            _db.Users.Find(_db.Users.Id == null);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[users] where [dbo].[users].[id] IS NULL");
        }

        [Test]
        public void TestFindWithTwoCriteriasOneBeingNull()
        {
            _db.Users.Find(_db.Users.Id == 1 || _db.Users.Id == null);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[users] where ([dbo].[users].[id] = @p1 OR [dbo].[users].[id] IS NULL)");
            Parameter(0).Is(1);
            Parameter(1).DoesNotExist();
        }

        [Test]
        public void TestFindNotEqualWithInt32()
        {
            _db.Users.Find(_db.Users.Id != 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] != @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindGreaterThanWithInt32()
        {
            _db.Users.Find(_db.Users.Id > 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] > @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindGreaterThanOrEqualWithInt32()
        {
            _db.Users.Find(_db.Users.Id >= 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] >= @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindLessThanWithInt32()
        {
            _db.Users.Find(_db.Users.Id < 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] < @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindLessThanOrEqualWithInt32()
        {
            _db.Users.Find(_db.Users.Id <= 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] <= @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindModulo()
        {
            _db.Users.Find(_db.Users.Id % 2 == 1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] % @p1 = @p2");
            Parameter(0).Is(2);
            Parameter(1).Is(1);
        }

        [Test]
        public void TestFindWithAdd()
        {
            _db.Users.Find(_db.Users.Id + _db.Users.Age == 42);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] + [dbo].[Users].[Age] = @p1");
            Parameter(0).Is(42);
        }

        [Test]
        public void TestFindWithSubtract()
        {
            _db.Users.Find(_db.Users.Id - _db.Users.Age == 42);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] - [dbo].[Users].[Age] = @p1");
            Parameter(0).Is(42);
        }

        [Test]
        public void TestFindWithMultiply()
        {
            _db.Users.Find(_db.Users.Id * _db.Users.Age == 42);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] * [dbo].[Users].[Age] = @p1");
            Parameter(0).Is(42);
        }

        [Test]
        public void TestFindWithDivide()
        {
            _db.Users.Find(_db.Users.Id / _db.Users.Age == 42);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[Id] / [dbo].[Users].[Age] = @p1");
            Parameter(0).Is(42);
        }
        
        [Test]
        public void TestFindByNamedParameterSingleColumn()
        {
            _db.Users.FindBy(Name: "Foo");
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[name] = @p1");
            Parameter(0).Is("Foo");
        }

        [Test]
        public void TestFindByNamedParameterTwoColumns()
        {
            _db.Users.FindBy(Name: "Foo", Password: "password");
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where ([dbo].[Users].[name] = @p1 and [dbo].[Users].[password] = @p2)");
            Parameter(0).Is("Foo");
            Parameter(1).Is("password");
        }

        [Test]
        public void TestFindByDynamicSingleColumn()
        {
            _db.Users.FindByName("Foo");
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[name] = @p1");
            Parameter(0).Is("Foo");
        }

        [Test]
        public void TestFindByDynamicSingleColumnNull()
        {
            _db.MyTable.FindByColumn1(null);
            GeneratedSqlIs("select [dbo].[MyTable].[Column1] from [dbo].[MyTable] where [dbo].[MyTable].[Column1] is null");
        }

        [Test]
        public void TestFindAllByDynamicSingleColumnNull()
        {
#pragma warning disable 168
            IEnumerable<MyTable> result = _db.MyTable.FindAllByColumn1(null).ToList<MyTable>();
#pragma warning restore 168
            GeneratedSqlIs("select [dbo].[MyTable].[Column1] from [dbo].[MyTable] where [dbo].[MyTable].[Column1] is null");
        }

        [Test]
        public void TestFindByWithAnonymousObject()
        {
            _db.MyTable.FindBy(new { Column1 = 1 });
            GeneratedSqlIs("select [dbo].[MyTable].[Column1] from [dbo].[MyTable] where [dbo].[MyTable].[Column1] = @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestFindByWithAnonymousObjectNullValue()
        {
            _db.MyTable.FindBy(new { Column1 = (object)null });
            GeneratedSqlIs("select [dbo].[MyTable].[Column1] from [dbo].[MyTable] where [dbo].[MyTable].[Column1] is null");
        }

        [Test]
        public void TestFindWithLike()
        {
            _db.Users.Find(_db.Users.Name.Like("Foo"));
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[name] like @p1");
            Parameter(0).Is("Foo");
        }

        [Test]
        public void TestFindWithNotLike()
        {
            _db.Users.Find(_db.Users.Name.NotLike("Foo"));
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where [dbo].[Users].[name] not like @p1");
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
        public void TestFindAllWithNotLike()
        {
            _db.Users.FindAll(_db.Users.Name.NotLike("Foo")).ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where [dbo].[Users].[name] not like @p1");
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
        public void TestFindAllByNamedParameterSingleColumnNull()
        {
            _db.Users.FindAllBy(Name: null).ToList();
            GeneratedSqlIs("select [dbo].[Users].[id],[dbo].[Users].[name],[dbo].[Users].[password],[dbo].[Users].[age] from [dbo].[Users] where [dbo].[Users].[name] is null");
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
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[Users] where ([dbo].[Users].[name] = @p1 and [dbo].[Users].[password] = @p2)");
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
        public void FindByWithoutArgsThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _db.Users.FindBy());
        }
 
        [Test]
        public void FindAllByWithoutArgsThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _db.Users.FindAllBy());
        }
    }

    class MyTable
    {
        public string Column1 { get; set; }
    }
}