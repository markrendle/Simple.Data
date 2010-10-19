using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestClass]
    public class NaturalJoinTest
    {
        static Database CreateDatabase()
        {
            return new Database(new MockConnectionProvider(new MockDbConnection(SchemaHelper.DummySchema())));
        }

        [TestMethod]
        public void NaturalJoinCreatesCorrectCommand()
        {
            // Arrange
            dynamic database = CreateDatabase();
            DateTime orderDate = new DateTime(2010, 1, 1);
            database.Customer.Find(database.Customer.Orders.OrderDate == orderDate);
            Assert.AreEqual("select * from Customer join Orders on Customer.CustomerId = Orders.CustomerId where Orders.OrderDate = @p1", MockDatabase.Sql);
            Assert.AreEqual(orderDate, MockDatabase.Parameters[0]);
        }
    }
}
