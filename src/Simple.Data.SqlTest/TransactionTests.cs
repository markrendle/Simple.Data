using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Simple.Data.Ado;

namespace Simple.Data.SqlTest
{
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class TransactionTests
    {
        public TransactionTests()
        {
            DatabaseHelper.Reset();
        }

        //TODO: [Fact]
        public async void TestCommit()
        {
            var db = DatabaseHelper.Open();

            using (var tx = db.BeginTransaction())
            {
                try
                {
                    var order = await tx.Orders.Insert(CustomerId: 1, OrderDate: DateTime.Today);
                    await tx.OrderItems.Insert(OrderId: order.OrderId, ItemId: 1, Quantity: 3);
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            Assert.AreEqual(2, db.Orders.All().ToList().Count);
            Assert.AreEqual(2, db.OrderItems.All().ToList().Count);
        }

        //TODO: [Fact]
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

        //TODO: [Fact]
        public void TestWithOptionsTransaction()
        {
            var dbWithOptions = DatabaseHelper.Open().WithOptions(new AdoOptions(commandTimeout: 60000));
            using (var tx = dbWithOptions.BeginTransaction())
            {
                tx.Orders.Insert(CustomerId: 1, OrderDate: DateTime.Today);
                tx.Rollback();
            }

            Assert.Pass();
        }

        //TODO: [Fact]
        public void TestRollbackOnProcedure()
        {
            var db = DatabaseHelper.Open();

            Customer customer;
            using (var tx = db.BeginTransaction())
            {
                customer = tx.CreateCustomer().FirstOrDefault();
                tx.Rollback();
            }
            customer = db.Customers.FindByCustomerId(customer.CustomerId);
            Assert.IsNull(customer);
        }

        //TODO: [Fact]
        public void QueryInsideTransaction()
        {
            var db = DatabaseHelper.Open();

            using (var tx = db.BeginTransaction())
            {
                tx.Users.Insert(Name: "Arthur", Age: 42, Password: "Ladida");
                User u2 = tx.Users.FindByName("Arthur");
                Assert.IsNotNull(u2);
                Assert.AreEqual("Arthur", u2.Name);
            }
        } 
    }
}
