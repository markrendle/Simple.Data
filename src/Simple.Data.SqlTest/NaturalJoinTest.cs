using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class NaturalJoinTest
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void CustomerDotOrdersDotOrderDateShouldReturnOneRow()
        {
            var db = DatabaseHelper.Open();
            var row = db.Customers.Find(db.Customers.Orders.OrderDate == new DateTime(2010, 10, 10));
            Assert.IsNotNull(row);
            Assert.AreEqual("Test", row.Name);
        }

        [Test]
        public void CustomerDotOrdersDotOrderItemsDotItemDotNameShouldReturnOneRow()
        {
            var db = DatabaseHelper.Open();
            var customer = db.Customers.Find(db.Customers.Orders.OrderItems.Item.Name == "Widget");
            Assert.IsNotNull(customer);
            Assert.AreEqual("Test", customer.Name);
            foreach (var order in customer.Orders)
            {
                Assert.AreEqual(1, order.OrderId);
            }
        }

        [Test]
        public void CustomerDotNameAndCustomerDotOrdersDotOrderItemsDotItemDotNameShouldReturnOneRow()
        {
            var db = DatabaseHelper.Open();
            var customer = db.Customers.Find(db.Customers.Name == "Test" &&
                                             db.Customers.Orders.OrderItems.Item.Name == "Widget");
            Assert.IsNotNull(customer);
            Assert.AreEqual("Test", customer.Name);
            foreach (var order in customer.Orders)
            {
                Assert.AreEqual(1, order.OrderId);
            }
        }

        [Test]
        public void CustomerDotNameAndCustomerDotOrdersDotOrderDateAndCustomerDotOrdersDotOrderItemsDotItemDotNameShouldReturnOneRow()
        {
            var db = DatabaseHelper.Open();
            var customer = db.Customers.Find(db.Customers.Name == "Test" &&
                                             db.Customers.Orders.OrderDate == new DateTime(2010, 10, 10) &&
                                             db.Customers.Orders.OrderItems.Item.Name == "Widget");
            Assert.IsNotNull(customer);
            Assert.AreEqual("Test", customer.Name);
            foreach (var order in customer.Orders)
            {
                Assert.AreEqual(1, order.OrderId);
            }
        }
    }
}
