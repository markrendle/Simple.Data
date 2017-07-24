namespace Shitty.Data.IntegrationTest.Query
{
    using System;
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class HavingTest : DatabaseIntegrationContext
    {
        [Test]
        public void HavingClauseWithNaturalJoin()
        {
            var q = _db.Customers.Query()
                .Having(_db.Customers.Orders.OrderDate.Max() < new DateTime(2011, 1, 1));

            EatException<InvalidOperationException>(() => q.ToList());

            GeneratedSqlIs("select [dbo].[customer].[customerid] from [dbo].[customer] " +
                           "join [dbo].[orders] on ([dbo].[customer].[customerid] = [dbo].[orders].[customerid]) " +
                           "group by [dbo].[customer].[customerid] " +
                           "having max([dbo].[orders].[orderdate]) < @p1");
        }

        [Test]
        public void HavingClauseWithDetailTableCount()
        {
            var q = _db.Customers.Query()
                .Having(_db.Customers.Orders.OrderId.Count() >= 100);

            EatException<InvalidOperationException>(() => q.ToList());

            GeneratedSqlIs("select [dbo].[customer].[customerid] from [dbo].[customer] " +
                           "join [dbo].[orders] on ([dbo].[customer].[customerid] = [dbo].[orders].[customerid]) " +
                           "group by [dbo].[customer].[customerid] " +
                           "having count([dbo].[orders].[orderid]) >= @p1");
        }

        [Test]
        public void BasicHavingClause()
        {
            var q = _db.Orders.Query()
                .Select(_db.Orders.CustomerId)
                .Having(_db.Orders.OrderDate.Max() <= new DateTime(2011, 1, 1));

            EatException<InvalidOperationException>(() => q.ToList());

            GeneratedSqlIs("select [dbo].[orders].[customerid] from [dbo].[orders] " +
                           "group by [dbo].[orders].[customerid] " +
                           "having max([dbo].[orders].[orderdate]) <= @p1");
        }

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
    }
}