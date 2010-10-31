using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class NaturalJoinTest
    {
        [Test]
        public void CustomerDotOrdersDotOrderDateShouldReturnOneRow()
        {
            var db = DatabaseHelper.Open();
            var row = db.Customers.Find(db.Customers.Orders.OrderDate == new DateTime(2010, 10, 10));
            Assert.AreEqual("Test", row.Name);
        }
    }
}
