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
    using Xunit;

    public class NaturalJoinTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Customer", "BASE TABLE" },
                                         new[] { "dbo", "Orders", "BASE TABLE" });
            schemaProvider.SetColumns(new[] { "dbo", "Customer", "CustomerId" },
                                          new[] { "dbo", "Orders", "OrderId" },
                                          new[] { "dbo", "Orders", "CustomerId" },
                                          new[] { "dbo", "Orders", "OrderDate" });
            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Customer", "CustomerId", 0 });
            schemaProvider.SetForeignKeys(new object[] { "FK_Orders_Customer", "dbo", "Orders", "CustomerId", "dbo", "Customer", "CustomerId", 0 });
        }

        [Fact]
        public async void NaturalJoinCreatesCorrectCommand()
        {
            var orderDate = new DateTime(2010, 1, 1);
            const string expectedSql =
                "select [dbo].[Customer].[CustomerId] from [dbo].[Customer] join [dbo].[Orders] on ([dbo].[Customer].[CustomerId] = [dbo].[Orders].[CustomerId]) where [dbo].[Orders].[OrderDate] = @p1";

            await TargetDb.Customer.Find(TargetDb.Customer.Orders.OrderDate == orderDate);
            GeneratedSqlIs(expectedSql);

            Parameter(0).Is(orderDate);
        }

        [Fact]
        public async void NaturalJoinViaFindAllCreatesCorrectCommand()
        {
            var orderDate = new DateTime(2010, 1, 1);
            const string expectedSql =
                "select [dbo].[Customer].[CustomerId] from [dbo].[Customer] join [dbo].[Orders] on ([dbo].[Customer].[CustomerId] = [dbo].[Orders].[CustomerId]) where [dbo].[Orders].[OrderDate] = @p1";

            await TargetDb.Customer.FindAll(TargetDb.Customer.Orders.OrderDate == orderDate).ToList<dynamic>();
            GeneratedSqlIs(expectedSql);

            Parameter(0).Is(orderDate);
        }
    }
}
