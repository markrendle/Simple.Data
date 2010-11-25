using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestFixture]
    public class NaturalJoinTest
    {
        static Database CreateDatabase(MockDatabase mockDatabase)
        {
            var mockSchemaProvider = new MockSchemaProvider();
            mockSchemaProvider.SetTables(new[] {"dbo", "Customer", "BASE TABLE"},
                                         new[] {"dbo", "Orders", "BASE TABLE"});
            mockSchemaProvider.SetColumns(new[] {"dbo", "Customer", "CustomerId"},
                                          new[] { "dbo", "Orders", "OrderId" },
                                          new[] { "dbo", "Orders", "CustomerId" },
                                          new[] {"dbo", "Orders", "OrderDate"});
            mockSchemaProvider.SetPrimaryKeys(new object[] {"dbo", "Customer", "CustomerId", 0});
            mockSchemaProvider.SetForeignKeys(new object[] {"FK_Orders_Customer", "dbo", "Orders", "CustomerId", "dbo", "Customer", "CustomerId", 0});
            return new Database(new AdoAdapter(new MockConnectionProvider(new MockDbConnection(mockDatabase), mockSchemaProvider)));
        }

        [Test]
        public void NaturalJoinCreatesCorrectCommand()
        {
            // Arrange
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            var orderDate = new DateTime(2010, 1, 1);
            const string expectedSql =
                "select [Customer].* from [Customer] join [Orders] on ([Customer].[CustomerId] = [Orders].[CustomerId]) where [Orders].[OrderDate] = @p1";

            // Act
            database.Customer.Find(database.Customer.Orders.OrderDate == orderDate);
            var actualSql = Regex.Replace(mockDatabase.Sql, @"\s+", " ").ToLowerInvariant();

            // Assert
            Assert.AreEqual(expectedSql.ToLowerInvariant(), actualSql);
            Assert.AreEqual(orderDate, mockDatabase.Parameters[0]);
        }
    }
}
