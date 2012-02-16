using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    using System;
    using System.Linq;

    [TestFixture]
    public class GetTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestGet()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.Get(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void WithClauseShouldCastToStaticTypeWithComplexProperty()
        {
            var db = DatabaseHelper.Open();
            Order actual = db.Orders.WithCustomer().Get(1);
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Customer);
            Assert.AreEqual("Test", actual.Customer.Name);
            Assert.AreEqual("100 Road", actual.Customer.Address);
        }

        [Test]
        public void WithClauseShouldCastToStaticTypeWithCollection()
        {
            var db = DatabaseHelper.Open();
            Customer actual = db.Customers.WithOrders().Get(1);
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Orders.Single().OrderId);
            Assert.AreEqual(new DateTime(2010, 10, 10), actual.Orders.Single().OrderDate);
        }
    }
}