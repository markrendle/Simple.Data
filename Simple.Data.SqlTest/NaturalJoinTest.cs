using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Simple.Data.SqlTest
{
    [TestClass]
    public class NaturalJoinTest
    {
        [TestMethod]
        public void CustomerDotOrdersDotOrderDateShouldReturnOneRow()
        {
            var db = DatabaseHelper.Open();
            var row = db.Customers.Find(db.Customers.Orders.OrderDate == new DateTime(2010, 10, 10));
            Assert.AreEqual("Test", row.Name);
        }
    }
}
