using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlTest
{
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class NaturalJoinTest
    {
        public NaturalJoinTest()
        {
            DatabaseHelper.Reset();
        }

        [Fact]
        public async void CustomerDotOrdersDotOrderDateShouldReturnOneRow()
        {
            var db = DatabaseHelper.Open();
            var row = await db.Customers.Find(db.Customers.Orders.OrderDate == new DateTime(2010, 10, 10));
            Assert.IsNotNull(row);
            Assert.AreEqual("Test", row.Name);
        }

        [Fact]
        public async void CustomerDotOrdersDotOrderItemsDotItemDotNameShouldReturnOneRow()
        {
            var db = DatabaseHelper.Open();
            var customer = await db.Customers.Find(db.Customers.Orders.OrderItems.Item.Name == "Widget");
            Assert.IsNotNull(customer);
            Assert.AreEqual("Test", customer.Name);
            foreach (var order in customer.Orders)
            {
                Assert.AreEqual(1, order.OrderId);
            }
        }

        [Fact]
        public async void CustomerDotNameAndCustomerDotOrdersDotOrderItemsDotItemDotNameShouldReturnOneRow()
        {
            var db = DatabaseHelper.Open();
            var customer = await db.Customers.Find(db.Customers.Name == "Test" &&
                                             db.Customers.Orders.OrderItems.Item.Name == "Widget");
            Assert.IsNotNull(customer);
            Assert.AreEqual("Test", customer.Name);
            foreach (var order in customer.Orders)
            {
                Assert.AreEqual(1, order.OrderId);
            }
        }

        [Fact]
        public async void CustomerDotNameAndCustomerDotOrdersDotOrderDateAndCustomerDotOrdersDotOrderItemsDotItemDotNameShouldReturnOneRow()
        {
            var db = DatabaseHelper.Open();
            var customer = await db.Customers.Find(db.Customers.Name == "Test" &&
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
