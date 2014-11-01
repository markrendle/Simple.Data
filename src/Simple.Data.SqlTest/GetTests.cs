using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    using System;
    using System.Linq;

    public class GetTests
    {
        public void Setup()
        {
            DatabaseHelper.Reset();
        }


        // TODO: Get needs rewriting to use SimpleQuery with deferred everything

        //TODO: [Fact]
        public void TestGet()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.Get(1);
            Assert.AreEqual(1, user.Id);
        }

        //TODO: [Fact]
        public void GetWithNonExistentPrimaryKeyShouldReturnNull()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.Get(1138);
            Assert.IsNull(user);
        }

        //TODO: [Fact]
        public void SelectClauseWithGetScalarShouldLimitQuery()
        {
            var db = DatabaseHelper.Open();
            string actual = db.Customers.Select(db.Customers.Name).GetScalar(1);
            Assert.IsNotNull(actual);
            Assert.AreEqual("Test", actual);
        }

        //TODO: [Fact]
        public void WithClauseShouldCastToStaticTypeWithComplexProperty()
        {
            var db = DatabaseHelper.Open();
            Order actual = db.Orders.WithCustomer().Get(1);
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Customer);
            Assert.AreEqual("Test", actual.Customer.Name);
            Assert.AreEqual("100 Road", actual.Customer.Address);
        }

        //TODO: [Fact]
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