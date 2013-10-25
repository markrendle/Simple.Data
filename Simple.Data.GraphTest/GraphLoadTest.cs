namespace Simple.Data.GraphTest
{
    using System;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Models;
    using NUnit.Framework;

    [TestFixture]
    public class GraphLoadTest
    {
        //[Test]
        public void LoadsEntireObjectGraphWithGet()
        {
            var db = Database.OpenNamedConnection("Graph");
            Customer customer = db.Customers.With(db.Customers.Orders).With(db.Customers.Orders.OrderItems.As("Items")).Get(1);
            Assert.AreEqual(1, customer.Orders.Count);
            var order = customer.Orders.Single();
            Assert.AreEqual(3, order.Items.Count);
        }
    }

    [SetUpFixture]
    public class SetupFixture
    {
        [SetUp]
        public void SetDataDirectoryToTestContext()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", TestContext.CurrentContext.TestDirectory);
        }
    }
}
