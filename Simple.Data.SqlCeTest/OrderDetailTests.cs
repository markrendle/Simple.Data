using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Simple.Data.SqlCeTest
{
    [TestClass]
    public class OrderDetailTests
    {
        private static readonly string DatabasePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)),
            "TestDatabase.sdf");

        [TestMethod]
        public void TestOrderDetail()
        {
            var db = Database.OpenFile(DatabasePath);
            var order = db.Orders.FindByOrderDate(DateTime.Today);
            IEnumerable<dynamic> orderItems = order.OrderItems;
            var orderItem = orderItems.FirstOrDefault();
            var item = orderItem.Item;
            Assert.AreEqual("Widget", item.Name);
        }
    }
}
