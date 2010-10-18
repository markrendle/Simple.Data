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
            return new Database(new MockConnectionProvider());
        }

        [TestMethod]
        public void NaturalJoinCreatesCorrectCommand()
        {
            dynamic database = CreateDatabase();
            database.Customer.Find(database.Customer.Orders.OrderDate == new DateTime(2010, 1, 1));
        }
    }
}
