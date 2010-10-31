using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestFixture]
    public class NaturalJoinTest
    {
        static Database CreateDatabase()
        {
            MockSchemaProvider.Reset();
            MockSchemaProvider.SetTables(new[] {"dbo", "Customer", "BASE TABLE"},
                                         new[] {"dbo", "Orders", "BASE TABLE"});
            MockSchemaProvider.SetColumns(new[] {"dbo", "Customer", "CustomerId"},
                                          new[] { "dbo", "Orders", "OrderId" },
                                          new[] { "dbo", "Orders", "CustomerId" },
                                          new[] {"dbo", "Orders", "OrderDate"});
            MockSchemaProvider.SetPrimaryKeys(new object[] {"dbo", "Customer", "CustomerId", 0});
            MockSchemaProvider.SetForeignKeys(new object[] {"FK_Orders_Customer", "dbo", "Orders", "CustomerId", "dbo", "Customer", "CustomerId", 0});
            return new Database(new MockConnectionProvider(new MockDbConnection()));
        }

        [Test]
        public void NaturalJoinCreatesCorrectCommand()
        {
            // Arrange
            dynamic database = CreateDatabase();
            var orderDate = new DateTime(2010, 1, 1);
            const string expectedSql =
                "select [Customer].* from [Customer] join [Orders] on ([Customer].[CustomerId] = [Orders].[CustomerId]) where [Orders].[OrderDate] = @p1";

            // Act
            database.Customer.Find(database.Customer.Orders.OrderDate == orderDate);
            var actualSql = Regex.Replace(MockDatabase.Sql, @"\s+", " ").ToLowerInvariant();

            // Assert
            Assert.AreEqual(expectedSql.ToLowerInvariant(), actualSql);
            Assert.AreEqual(orderDate, MockDatabase.Parameters[0]);
        }
    }
}
