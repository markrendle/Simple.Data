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

    /// <summary>
    /// Summary description for FindTests
    /// </summary>
    [TestFixture]
    public class FindTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestFindById()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestFindByIdWithCast()
        {
            var db = DatabaseHelper.Open();
            var user = (User)db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestFindByReturnsOne()
        {
            var db = DatabaseHelper.Open();
            var user = (User)db.Users.FindByName("Bob");
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestFindAllByName()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAllByName("Bob").Cast<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Test]
        public void TestFindAllByNameAsIEnumerableOfDynamic()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<dynamic> users = db.Users.FindAllByName("Bob");
            Assert.AreEqual(1, users.Count());
        }

        [Test]
        public void TestFindAllByPartialName()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Name.Like("Bob")).ToList<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Test]
        public void TestFindAllByPartialNameOnChar()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.UsersWithChar.FindAll(db.UsersWithChar.Name.Like("Bob%")).ToList<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Test]
        public void TestFindAllByExcludedPartialName()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Name.NotLike("Bob")).ToList<User>();
            Assert.AreEqual(2, users.Count());
        }

        [Test]
        public void TestAllCount()
        {
            var db = DatabaseHelper.Open();
            var count = db.Users.All().ToList().Count;
            Assert.AreEqual(3, count);
        }

        [Test]
        public void TestAllWithSkipCount()
        {
            var db = DatabaseHelper.Open();
            var count = db.Users.All().Skip(1).ToList().Count;
            Assert.AreEqual(2, count);
        }

        [Test]
        public void TestImplicitCast()
        {
            var db = DatabaseHelper.Open();
            User user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestImplicitEnumerableCast()
        {
            var db = DatabaseHelper.Open();
            foreach (User user in db.Users.All())
            {
                Assert.IsNotNull(user);
            }
        }

        
        [Test]
        public void TestFindWithCriteriaAndSchemaQualification()
        {
            var db = DatabaseHelper.Open();

            var dboActual = db.dbo.SchemaTable.Find(db.dbo.SchemaTable.Id == 1);

            Assert.IsNotNull(dboActual);
            Assert.AreEqual("Pass", dboActual.Description);
        }

        [Test]
        public void TestFindOnAView()
        {
            var db = DatabaseHelper.Open();
            var u = db.VwCustomers.FindByCustomerId(1);
            Assert.IsNotNull(u);
        }

        [Test]
        public void TestCast()
        {
            var db = DatabaseHelper.Open();
            var userQuery = db.Users.All().Cast<User>() as IEnumerable<User>;
            Assert.IsNotNull(userQuery);
            var users = userQuery.ToList();
            Assert.AreNotEqual(0, users.Count);
        }

        [Test]
        public void FindByWithNamedParameter()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.FindBy(Name: "Bob");
            Assert.IsNotNull(user);

        }

        [Test]
        public void WithClauseShouldCastToStaticTypeWithCollection()
        {
            var db = DatabaseHelper.Open();
            Customer actual = db.Customers.WithOrders().FindByCustomerId(1);
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Orders.Single().OrderId);
            Assert.AreEqual(new DateTime(2010, 10, 10), actual.Orders.Single().OrderDate);
        }

        [Test]
        public void NamedParameterAndWithClauseShouldCastToStaticTypeWithCollection()
        {
            var db = DatabaseHelper.Open();
            Customer actual = db.Customers.WithOrders().FindBy(CustomerId: 1);
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Orders.Single().OrderId);
            Assert.AreEqual(new DateTime(2010, 10, 10), actual.Orders.Single().OrderDate);
        }

        [Test]
        public void ExpressionAndWithClauseShouldCastToStaticTypeWithCollection()
        {
            var db = DatabaseHelper.Open();
            Customer actual = db.Customers.WithOrders().Find(db.Customers.CustomerId == 1);
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Orders.Single().OrderId);
            Assert.AreEqual(new DateTime(2010, 10, 10), actual.Orders.Single().OrderDate);
        }

        [Test]
        public void SelectClauseShouldRestrictColumn()
        {
            var db = DatabaseHelper.Open();
            var actual = db.Customers.Select(db.Customers.Name).FindByCustomerId(1).ToScalar();
            Assert.AreEqual("Test", actual);

        }
    }
}
