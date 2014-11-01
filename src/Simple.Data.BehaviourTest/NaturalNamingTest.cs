using System;
using NUnit.Framework;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    using Xunit;

    public class NaturalNamingTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(
                new[] { "dbo", "Customers", "BASE TABLE" },
                new[] { "dbo", "Customer_Orders", "BASE TABLE" });
            schemaProvider.SetColumns(
                new[] { "dbo", "Customers", "CustomerId" },
                new[] { "dbo", "Customers", "Customer_Name" },
                new[] { "dbo", "Customer_Orders", "ORDER_ID" },
                new[] { "dbo", "Customer_Orders", "CUSTOMER_ID" },
                new[] { "dbo", "Customer_Orders", "ORDER_DATE" });
            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Customers", "CustomerId", 0 });
            schemaProvider.SetForeignKeys(new object[] { "FK_Orders_Customers", "dbo", "Orders", "CustomerId", "dbo", "Customers", "CustomerId", 0 });
        }

        [Fact]
        public async void DotNetNamingIsCorrectlyResolvedInFind()
        {
            await TargetDb.Customers.Find(TargetDb.Customers.CustomerName == "Arthur");
            GeneratedSqlIs("select [dbo].[Customers].[CustomerId],[dbo].[Customers].[Customer_Name] from [dbo].[customers] where [dbo].[customers].[customer_name] = @p1");
            Parameter(0).Is("Arthur");
        }

        [Fact]
        public void DotNetNamingIsCorrectlyResolvedInOrderBy()
        {
            TargetDb.Customers.FindAll(TargetDb.Customers.CustomerName == "Arthur").OrderByCustomerName().ToList<dynamic>();
            GeneratedSqlIs("select [dbo].[customers].[customerid],[dbo].[customers].[customer_name] from [dbo].[customers] where [dbo].[customers].[customer_name] = @p1 order by [dbo].[customers].[customer_name]");
            Parameter(0).Is("Arthur");
        }

        [Fact]
        public void DotNetNamingIsCorrectlyResolvedInOrderByExpression()
        {
            TargetDb.Customers.FindAll(TargetDb.Customers.CustomerName == "Arthur").OrderBy(TargetDb.Customers.CustomerName).ToList<dynamic>();
            GeneratedSqlIs("select [dbo].[customers].[customerid],[dbo].[customers].[customer_name] from [dbo].[customers] where [dbo].[customers].[customer_name] = @p1 order by [dbo].[customers].[customer_name]");
            Parameter(0).Is("Arthur");
        }

        [Fact]
        public void SortOrderIsCorrectlyResolvedInOrderByExpression()
        {
            TargetDb.Customers.FindAll(TargetDb.Customers.CustomerName == "Arthur").OrderBy(TargetDb.Customers.CustomerName, OrderByDirection.Descending).ToList<dynamic>();
            GeneratedSqlIs("select [dbo].[customers].[customerid],[dbo].[customers].[customer_name] from [dbo].[customers] where [dbo].[customers].[customer_name] = @p1 order by [dbo].[customers].[customer_name] desc");
            Parameter(0).Is("Arthur");
        }

        [Fact]
        public void SortOrderIsCorrectlyResolvedInOrderByThenByExpression()
        {
            TargetDb.Customers.FindAll(TargetDb.Customers.CustomerName == "Arthur").OrderBy(TargetDb.Customers.CustomerName, OrderByDirection.Descending).ThenBy(TargetDb.Customers.CustomerId, OrderByDirection.Ascending).ToList<dynamic>();
            GeneratedSqlIs("select [dbo].[customers].[customerid],[dbo].[customers].[customer_name] from [dbo].[customers] where [dbo].[customers].[customer_name] = @p1 order by [dbo].[customers].[customer_name] desc, [dbo].[customers].[customerid]");
            Parameter(0).Is("Arthur");
        }

        [Fact]
        public void DotNetNamingInComplexQuery()
        {
            TargetDb.CustomerOrders.FindAll(TargetDb.CustomerOrders.CustomerId.Like("123%") || (TargetDb.CustomerOrders.CustomerId == null)).ToList<dynamic>();
            GeneratedSqlIs("select [dbo].[Customer_Orders].[ORDER_ID],[dbo].[Customer_Orders].[CUSTOMER_ID],[dbo].[Customer_Orders].[ORDER_DATE] from [dbo].[Customer_Orders] where ([dbo].[Customer_Orders].[CUSTOMER_ID] like @p1 OR [dbo].[Customer_Orders].[CUSTOMER_ID] IS NULL)");
            Parameter(0).Is("123%");
        }
    }
}