using System;
using NUnit.Framework;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    public class NaturalNamingTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(
                new[] { "dbo", "Customers", "BASE TABLE" },
                new[] { "dbo", "Orders", "BASE TABLE" });
            schemaProvider.SetColumns(
                new[] { "dbo", "Customers", "CustomerId" },
                new[] { "dbo", "Customers", "Customer_Name" },
                new[] { "dbo", "Orders", "OrderId" },
                new[] { "dbo", "Orders", "CustomerId" },
                new[] { "dbo", "Orders", "OrderDate" });
            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Customers", "CustomerId", 0 });
            schemaProvider.SetForeignKeys(new object[] { "FK_Orders_Customers", "dbo", "Orders", "CustomerId", "dbo", "Customers", "CustomerId", 0 });
        }

        [Test]
        public void DotNetNamingIsCorrectlyResolvedInFind()
        {
            _db.Customers.Find(_db.Customers.CustomerName == "Arthur");
            GeneratedSqlIs("select [dbo].[customers].* from [dbo].[customers] where [dbo].[customers].[customer_name] = @p1");
            Parameter(0).Is("Arthur");
        }

        [Test]
        public void DotNetNamingIsCorrectlyResolvedInOrderBy()
        {
            _db.Customers.FindAll(_db.Customers.CustomerName == "Arthur").OrderByCustomerName().ToList<dynamic>();
            GeneratedSqlIs("select [dbo].[customers].[customerid],[dbo].[customers].[customer_name] from [dbo].[customers] where [dbo].[customers].[customer_name] = @p1 order by [customer_name]");
            Parameter(0).Is("Arthur");
        }

        [Test]
        public void DotNetNamingIsCorrectlyResolvedInOrderByExpression()
        {
            _db.Customers.FindAll(_db.Customers.CustomerName == "Arthur").OrderBy(_db.Customers.CustomerName).ToList<dynamic>();
            GeneratedSqlIs("select [dbo].[customers].[customerid],[dbo].[customers].[customer_name] from [dbo].[customers] where [dbo].[customers].[customer_name] = @p1 order by [customer_name]");
            Parameter(0).Is("Arthur");
        }
    }
}