using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class TransactionTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            using (var cn = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                using (var cmd = new SqlCommand(Properties.Resources.DatabaseReset, cn))
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [Test]
        public void TestCommit()
        {
            var db = DatabaseHelper.Open();

            using (var tx = db.BeginTransaction())
            {
                var order = tx.Orders.Insert(CustomerId: 1, OrderDate: DateTime.Today);
                tx.OrderItems.Insert(OrderId: order.OrderId, ItemId: 1, Quantity: 3);
                tx.Commit();
            }
            Assert.AreEqual(2, db.Orders.All().ToList().Count);
            Assert.AreEqual(2, db.OrderItems.All().ToList().Count);
        }

        [Test]
        public void TestRollback()
        {
            var db = DatabaseHelper.Open();

            using (var tx = db.BeginTransaction())
            {
                var order = tx.Orders.Insert(CustomerId: 1, OrderDate: DateTime.Today);
                tx.OrderItems.Insert(OrderId: order.OrderId, ItemId: 1, Quantity: 3);
                tx.Rollback();
            }
            Assert.AreEqual(1, db.Orders.All().ToList().Count);
            Assert.AreEqual(1, db.OrderItems.All().ToList().Count);
        }
    }
}
