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

            IEnumerable<dynamic> orderItems = order.OrderItems;
            var orderItem = orderItems.FirstOrDefault();
            var item = orderItem.Item;
            Assert.IsNotNull(item);
            Assert.AreEqual("Widget", item.Name);
        }
    }
}
