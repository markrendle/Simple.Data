using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestClass]
    public class NameResolutionTests
    {
        static Database CreateDatabaseWithSingularNames()
        {
            MockSchemaProvider.SetTables(new[] { "dbo", "Customer", "BASE TABLE" },
                                         new[] { "dbo", "Orders", "BASE TABLE" });
            MockSchemaProvider.SetColumns(new[] { "dbo", "Customer", "CustomerId" },
                                          new[] { "dbo", "Orders", "OrderId" },
                                          new[] { "dbo", "Orders", "CustomerId" },
                                          new[] { "dbo", "Orders", "OrderDate" });
            MockSchemaProvider.SetPrimaryKeys(new object[] { "dbo", "Customer", "CustomerId", 0 });
            MockSchemaProvider.SetForeignKeys(new object[] { "dbo", "Orders", "CustomerId", "dbo", "Customer", "CustomerId", 0 });
            return new Database(new MockConnectionProvider(new MockDbConnection(SchemaHelper.DummySchema())));
        }

        [TestMethod]
        public void NaturalJoinCreatesCorrectCommand()
        {
            // Arrange
            dynamic database = CreateDatabaseWithSingularNames();
            DateTime orderDate = new DateTime(2010, 1, 1);
            const string expectedSql = "select Customer.* from Customer join Orders on (Customer.CustomerId = Orders.CustomerId) where Orders.OrderDate = @p1";

            // Act
            database.Customer.Find(database.Customers.Orders.OrderDate == orderDate);


            // Assert
            Assert.AreEqual(expectedSql, MockDatabase.Sql, true);
            Assert.AreEqual(orderDate, MockDatabase.Parameters[0]);
        }

        static Database CreateDatabaseWithShoutyNames()
        {
            MockSchemaProvider.SetTables(new[] { "dbo", "CUSTOMER", "BASE TABLE" },
                                         new[] { "dbo", "ORDER", "BASE TABLE" });
            MockSchemaProvider.SetColumns(new[] { "dbo", "CUSTOMER", "CUSTOMER_ID" },
                                          new[] { "dbo", "ORDER", "ORDER_ID" },
                                          new[] { "dbo", "ORDER", "CUSTOMER_ID" },
                                          new[] { "dbo", "ORDER", "ORDER_DATE" });
            MockSchemaProvider.SetPrimaryKeys(new object[] { "dbo", "CUSTOMER", "CUSTOMER_ID", 0 });
            MockSchemaProvider.SetForeignKeys(new object[] { "dbo", "ORDER", "CUSTOMER_ID", "dbo", "CUSTOMER", "CUSTOMER_ID", 0 });
            return new Database(new MockConnectionProvider(new MockDbConnection(SchemaHelper.DummySchema())));
        }

        [TestMethod]
        public void NaturalJoinWithShoutyCaseCreatesCorrectCommand()
        {
            // Arrange
            dynamic database = CreateDatabaseWithShoutyNames();
            DateTime orderDate = new DateTime(2010, 1, 1);
            const string expectedSql = "select CUSTOMER.* from CUSTOMER join ORDER on (CUSTOMER.CUSTOMER_ID = ORDER.CUSTOMER_ID)"
            + " where ORDER.ORDER_DATE = @p1";

            // Act
            database.Customer.Find(database.Customers.Orders.OrderDate == orderDate);


            // Assert
            Assert.AreEqual(expectedSql, MockDatabase.Sql, true);
            Assert.AreEqual(orderDate, MockDatabase.Parameters[0]);
        }
    }
}
