using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestFixture]
    public class NameResolutionTests
    {
        Database CreateDatabaseWithSingularNames(MockDatabase mockDatabase)
        {
            var mockSchemaProvider = new MockSchemaProvider();
            mockSchemaProvider.SetTables(new[] { "dbo", "Customer", "BASE TABLE" },
                                         new[] { "dbo", "Orders", "BASE TABLE" });
            mockSchemaProvider.SetColumns(new[] { "dbo", "Customer", "CustomerId" },
                                          new[] { "dbo", "Orders", "OrderId" },
                                          new[] { "dbo", "Orders", "CustomerId" },
                                          new[] { "dbo", "Orders", "OrderDate" });
            mockSchemaProvider.SetPrimaryKeys(new object[] { "dbo", "Customer", "CustomerId", 0 });
            mockSchemaProvider.SetForeignKeys(new object[] { "FK_Orders_Customer", "dbo", "Orders", "CustomerId", "dbo", "Customer", "CustomerId", 0 });
            return new Database(new AdoAdapter(new MockConnectionProvider(new MockDbConnection(mockDatabase), mockSchemaProvider)));
        }

        Database CreateDatabaseWithPluralNames(MockDatabase mockDatabase)
        {
            var mockSchemaProvider = new MockSchemaProvider();
            mockSchemaProvider.SetTables(new[] { "dbo", "Customers", "BASE TABLE" },
                                         new[] { "dbo", "Orders", "BASE TABLE" });
            mockSchemaProvider.SetColumns(new[] { "dbo", "Customers", "CustomerId" },
                                          new[] { "dbo", "Orders", "OrderId" },
                                          new[] { "dbo", "Orders", "CustomerId" },
                                          new[] { "dbo", "Orders", "OrderDate" });
            mockSchemaProvider.SetPrimaryKeys(new object[] { "dbo", "Customers", "CustomerId", 0 });
            mockSchemaProvider.SetForeignKeys(new object[] { "FK_Orders_Customers", "dbo", "Orders", "CustomerId", "dbo", "Customers", "CustomerId", 0 });
            return new Database(new AdoAdapter(new MockConnectionProvider(new MockDbConnection(mockDatabase), mockSchemaProvider)));
        }

        Database CreateDatabaseWithShoutyNames(MockDatabase mockDatabase)
        {
            var mockSchemaProvider = new MockSchemaProvider();
            mockSchemaProvider.SetTables(new[] { "dbo", "CUSTOMER", "BASE TABLE" },
                                         new[] { "dbo", "ORDER", "BASE TABLE" });
            mockSchemaProvider.SetColumns(new[] { "dbo", "CUSTOMER", "CUSTOMER_ID" },
                                          new[] { "dbo", "ORDER", "ORDER_ID" },
                                          new[] { "dbo", "ORDER", "CUSTOMER_ID" },
                                          new[] { "dbo", "ORDER", "ORDER_DATE" });
            mockSchemaProvider.SetPrimaryKeys(new object[] { "dbo", "CUSTOMER", "CUSTOMER_ID", 0 });
            mockSchemaProvider.SetForeignKeys(new object[] { "FK_ORDER_CUSTOMER", "dbo", "ORDER", "CUSTOMER_ID", "dbo", "CUSTOMER", "CUSTOMER_ID", 0 });
            return new Database(new AdoAdapter(new MockConnectionProvider(new MockDbConnection(mockDatabase), mockSchemaProvider)));
        }

        [Test]
        public void IndexerMethodWorksWithPluralFromSingular()
        {
            // Arrange
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabaseWithPluralNames(mockDatabase);
            string expectedSql =
                "select * from [dbo].[Customers]".ToLowerInvariant();

            // Act
            database["Customer"].All();

            // Assert
            Assert.AreEqual(expectedSql, mockDatabase.Sql.ToLowerInvariant());
        }

        [Test]
        public void IndexerMethodWorksWithShoutyFromSingular()
        {
            // Arrange
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabaseWithShoutyNames(mockDatabase);
            string expectedSql =
                "select * from [dbo].[CUSTOMER]".ToLowerInvariant();

            // Act
            database["Customer"].All();

            // Assert
            Assert.AreEqual(expectedSql, mockDatabase.Sql.ToLowerInvariant());
        }

        [Test]
        public void IndexerMethodWorksWithSchemaAndShoutyFromSingular()
        {
            // Arrange
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabaseWithShoutyNames(mockDatabase);
            string expectedSql =
                "select * from [dbo].[CUSTOMER]".ToLowerInvariant();

            // Act
            database["dbo"]["Customer"].All();

            // Assert
            Assert.AreEqual(expectedSql, mockDatabase.Sql.ToLowerInvariant());
        }

        [Test]
        public void IndexerMethodWorksWithSchemaAndPluralFromSingular()
        {
            // Arrange
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabaseWithPluralNames(mockDatabase);
            string expectedSql =
                "select * from [dbo].[Customers]".ToLowerInvariant();

            // Act
            database["dbo"]["Customer"].All();

            // Assert
            Assert.AreEqual(expectedSql, mockDatabase.Sql.ToLowerInvariant());
        }

        [Test]
        public void NaturalJoinWithIndexersCreatesCorrectCommand()
        {
            // Arrange
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabaseWithSingularNames(mockDatabase);
            var orderDate = new DateTime(2010, 1, 1);
            string expectedSql =
                "select [dbo].[Customer].* from [dbo].[Customer] join [dbo].[Orders] on ([dbo].[Customer].[CustomerId] = [dbo].[Orders].[CustomerId]) where [dbo].[Orders].[OrderDate] = @p1".ToLowerInvariant();

            // Act
            database["Customer"].Find(database["Customers"]["Orders"]["OrderDate"] == orderDate);


            // Assert
            Assert.AreEqual(expectedSql, mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(orderDate, mockDatabase.Parameters[0]);
        }

        [Test]
        public void NaturalJoinCreatesCorrectCommand()
        {
            // Arrange
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabaseWithSingularNames(mockDatabase);
            var orderDate = new DateTime(2010, 1, 1);
            string expectedSql =
                "select [dbo].[Customer].* from [dbo].[Customer] join [dbo].[Orders] on ([dbo].[Customer].[CustomerId] = [dbo].[Orders].[CustomerId]) where [dbo].[Orders].[OrderDate] = @p1".ToLowerInvariant();

            // Act
            database.Customer.Find(database.Customers.Orders.OrderDate == orderDate);


            // Assert
            Assert.AreEqual(expectedSql, mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(orderDate, mockDatabase.Parameters[0]);
        }

        [Test]
        public void NaturalJoinWithShoutyCaseCreatesCorrectCommand()
        {
            // Arrange
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabaseWithShoutyNames(mockDatabase);
            var orderDate = new DateTime(2010, 1, 1);
            string expectedSql = "select [dbo].[CUSTOMER].* from [dbo].[CUSTOMER] join [dbo].[ORDER] on ([dbo].[CUSTOMER].[CUSTOMER_ID] = [dbo].[ORDER].[CUSTOMER_ID])"
                                 + " where [dbo].[ORDER].[ORDER_DATE] = @p1";

            // Act
            database.Customer.Find(database.Customers.Orders.OrderDate == orderDate);


            // Assert
            Assert.AreEqual(expectedSql.ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(orderDate, mockDatabase.Parameters[0]);
        }
    }
}
