using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestClass]
    public class NaturalJoinTest
    {
        static Database CreateDatabase()
        {
            MockSchemaProvider.SetTables(new[] {"dbo", "Customer", "BASE TABLE"},
                                         new[] {"dbo", "Orders", "BASE TABLE"});
            MockSchemaProvider.SetColumns(new[] {"dbo", "Customer", "CustomerId"},
                                          new[] { "dbo", "Orders", "OrderId" },
                                          new[] { "dbo", "Orders", "CustomerId" },
                                          new[] {"dbo", "Orders", "OrderDate"});
            MockSchemaProvider.SetPrimaryKeys(new object[] {"dbo", "Customer", "CustomerId", 0});
            MockSchemaProvider.SetForeignKeys(new object[] {"dbo", "Orders", "CustomerId", "dbo", "Customer", "CustomerId", 0});
            return new Database(new MockConnectionProvider(new MockDbConnection(SchemaHelper.DummySchema())));
        }

        [TestMethod]
        public void NaturalJoinCreatesCorrectCommand()
        {
            // Arrange
            dynamic database = CreateDatabase();
            DateTime orderDate = new DateTime(2010, 1, 1);
            var expectedSql =
                "select Customer.* from Customer join Orders on (Customer.CustomerId = Orders.CustomerId) where Orders.OrderDate = @p1";

            // Act
            database.Customer.Find(database.Customer.Orders.OrderDate == orderDate);
            var actualSql = Regex.Replace(MockDatabase.Sql, @"\s+", " ");

            // Assert
            Assert.AreEqual(expectedSql, actualSql, true);
            Assert.AreEqual(orderDate, MockDatabase.Parameters[0]);
        }
    }
}
