using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class OrderDetailTests
    {
        [Test]
        public void TestOrderDetail()
        {
            var db = DatabaseHelper.Open();
            var order = db.Orders.FindByOrderDate(new DateTime(2010,10,10));
            Assert.IsNotNull(order);

            var orderItem = order.OrderItems.FirstOrDefault();
            var item = orderItem.Item;
            Assert.IsNotNull(item);
            Assert.AreEqual("Widget", item.Name);
        }
        
        [Test]
        public void TestOrderDetailFromList()
        {
            var db = DatabaseHelper.Open();
            var orders = db.Orders.FindAllByOrderDate(new DateTime(2010,10,10));
            Assert.IsNotNull(orders);

            foreach (var order in orders)
            {
                var orderItem = order.OrderItems.FirstOrDefault();
                var item = orderItem.Item;
                Assert.IsNotNull(item);
                Assert.AreEqual("Widget", item.Name);
            }
        }

        [Test]
        public void TestComplexObjectCreation()
        {
            var db = DatabaseHelper.Open();
            var row = db.Customers.FindByCustomerId(1);
            Customer customer = row;
            customer.Orders.AddRange(row.Orders.Cast<Order>());

            Assert.AreEqual("Test", customer.Name);
            Assert.AreEqual(1, customer.Orders.Count);
            Assert.AreEqual(1, customer.Orders.First().OrderId);
            Assert.AreEqual(1, customer.Orders.First().CustomerId);
            Assert.AreEqual(new DateTime(2010, 10, 10), customer.Orders.First().OrderDate);
        }
    }

    class Customer
    {
        private readonly List<Order> _orders = new List<Order>();

        public List<Order> Orders
        {
            get { return _orders; }
        }

        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

    class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
