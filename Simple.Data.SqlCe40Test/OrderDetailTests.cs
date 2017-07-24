using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Shitty.Data.SqlCeTest
{
    [TestFixture]
    public class OrderDetailTests
    {
        private static readonly string DatabasePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)),
            "TestDatabase.sdf");

        [Test]
        public void TestOrderDetail()
        {
            var db = Database.OpenFile(DatabasePath);
            var order = db.Orders.FindByOrderDate(new DateTime(2010, 8, 11));
            Assert.IsNotNull(order);
            var orderItem = order.OrderItems.FirstOrDefault();
            Assert.IsNotNull(orderItem);
            var item = orderItem.Item;
            Assert.AreEqual("Widget", item.Name);
        }
    }
}
