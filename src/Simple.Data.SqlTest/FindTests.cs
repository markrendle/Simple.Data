using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using System.IO;
using Simple.Data.Ado;
using Simple.Data.SqlServer;
using Simple.Data.TestHelper;

namespace Simple.Data.SqlTest
{
    using System;
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    /// <summary>
    /// Summary description for FindTests
    /// </summary>
    public class FindTests
    {
        public FindTests()
        {
            DatabaseHelper.Reset();
        }
        public async void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Fact]
        public async void TestFindById()
        {
            var db = DatabaseHelper.Open();
            var user = await db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Fact]
        public async void TestFindByIdWithCast()
        {
            var db = DatabaseHelper.Open();
            var user = (User)(await db.Users.FindById(1));
            Assert.AreEqual(1, user.Id);
        }

        [Fact]
        public async void TestFindByReturnsOne()
        {
            var db = DatabaseHelper.Open();
            var user = (User)(await db.Users.FindByName("Bob"));
            Assert.AreEqual(1, user.Id);
        }

        [Fact]
        public async void TestFindAllByName()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = await db.Users.FindAllByName("Bob").Cast<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Fact]
        public async void TestFindAllByNameArray()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = await db.Users.FindAllByName(new[] { "Bob", "UnknownUser" }).Cast<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Fact]
        public async void TestFindAllByNameAsIEnumerableOfDynamic()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<dynamic> users = await db.Users.FindAllByName("Bob");
            Assert.AreEqual(1, users.Count());
        }

        [Fact]
        public async void TestFindAllByPartialName()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = await db.Users.FindAll(db.Users.Name.Like("Bob")).ToList<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Fact]
        public async void TestFindAllByPartialNameOnChar()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = await db.UsersWithChar.FindAll(db.UsersWithChar.Name.Like("Bob%")).ToList<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Fact]
        public async void TestFindAllByExcludedPartialName()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = await db.Users.FindAll(db.Users.Name.NotLike("Bob")).ToList<User>();
            Assert.AreEqual(2, users.Count());
        }

        [Fact]
        public async void TestAllCount()
        {
            var db = DatabaseHelper.Open();
            var count = (await db.Users.All().ToList()).Count;
            Assert.AreEqual(3, count);
        }

        [Fact]
        public async void TestAllWithSkipCount()
        {
            var db = DatabaseHelper.Open();
            var count = await db.Users.All().Skip(1).ToList().Count;
            Assert.AreEqual(2, count);
        }

        [Fact]
        public async void TestImplicitCast()
        {
            var db = DatabaseHelper.Open();
            User user = await db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Fact]
        public async void TestImplicitEnumerableCast()
        {
            var db = DatabaseHelper.Open();
            foreach (User user in await db.Users.All())
            {
                Assert.IsNotNull(user);
            }
        }

        
        [Fact]
        public async void TestFindWithCriteriaAndSchemaQualification()
        {
            var db = DatabaseHelper.Open();

            var dboActual = await db.dbo.SchemaTable.Find(db.dbo.SchemaTable.Id == 1);

            Assert.IsNotNull(dboActual);
            Assert.AreEqual("Pass", dboActual.Description);
        }

        [Fact]
        public async void TestFindOnAView()
        {
            var db = DatabaseHelper.Open();
            var u = await db.VwCustomers.FindByCustomerId(1);
            Assert.IsNotNull(u);
        }

        [Fact]
        public async void TestCast()
        {
            var db = DatabaseHelper.Open();
            var userQuery = (await db.Users.All().Cast<User>()) as IEnumerable<User>;
            Assert.IsNotNull(userQuery);
            var users = userQuery.ToList();
            Assert.AreNotEqual(0, users.Count);
        }

        //TODO: [Fact]
        public async void FindByWithNamedParameter()
        {
            var db = DatabaseHelper.Open();
            var user = await db.Users.FindBy(Name: "Bob");
            Assert.IsNotNull(user);

        }

        //TODO: [Fact]
        public async void WithClauseShouldCastToStaticTypeWithCollection()
        {
            var db = DatabaseHelper.Open();
            Customer actual = await db.Customers.WithOrders().FindByCustomerId(1);
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Orders.Single().OrderId);
            Assert.AreEqual(new DateTime(2010, 10, 10), actual.Orders.Single().OrderDate);
        }

        //TODO: [Fact]
        public async void NamedParameterAndWithClauseShouldCastToStaticTypeWithCollection()
        {
            var db = DatabaseHelper.Open();
            Customer actual = await db.Customers.WithOrders().FindBy(CustomerId: 1);
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Orders.Single().OrderId);
            Assert.AreEqual(new DateTime(2010, 10, 10), actual.Orders.Single().OrderDate);
        }

        //TODO: [Fact]
        public async void ExpressionAndWithClauseShouldCastToStaticTypeWithCollection()
        {
            var db = DatabaseHelper.Open();
            Customer actual = await db.Customers.WithOrders().Find(db.Customers.CustomerId == 1);
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Orders.Single().OrderId);
            Assert.AreEqual(new DateTime(2010, 10, 10), actual.Orders.Single().OrderDate);
        }

        //TODO: [Fact]
        public async void SelectClauseShouldRestrictColumn()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.Customers.Select(db.Customers.Name).FindByCustomerId(1).ToScalar();
            Assert.AreEqual("Test", actual);

        }
    }
}
