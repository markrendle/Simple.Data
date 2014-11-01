namespace Simple.Data.IntegrationTest
{
    using System;
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class ShoutyNameResolutionTests : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "CUSTOMER", "BASE TABLE" },
                new[] { "dbo", "ORDER", "BASE TABLE" });
            schemaProvider.SetColumns(new[] { "dbo", "CUSTOMER", "CUSTOMER_ID" },
                new[] { "dbo", "ORDER", "ORDER_ID" },
                new[] { "dbo", "ORDER", "CUSTOMER_ID" },
                new[] { "dbo", "ORDER", "ORDER_DATE" });
            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "CUSTOMER", "CUSTOMER_ID", 0 });
            schemaProvider.SetForeignKeys(new object[] { "FK_ORDER_CUSTOMER", "dbo", "ORDER", "CUSTOMER_ID", "dbo", "CUSTOMER", "CUSTOMER_ID", 0 });
        }

        [Test]
        public void IndexerMethodWorksWithShoutyFromSingular()
        {
            TargetDb["Customer"].All().ToList();
            GeneratedSqlIs("select [dbo].[CUSTOMER].[CUSTOMER_ID] from [dbo].[CUSTOMER]");
        }

        [Test]
        public void IndexerMethodWorksWithSchemaAndShoutyFromSingular()
        {
            TargetDb["dbo"]["Customer"].All().ToList();
            GeneratedSqlIs("select [dbo].[CUSTOMER].[CUSTOMER_ID] from [dbo].[CUSTOMER]");
        }


        [Test]
        public void NaturalJoinWithShoutyCaseCreatesCorrectCommand()
        {
            var orderDate = new DateTime(2010, 1, 1);
            TargetDb.Customer.Find(TargetDb.Customers.Orders.OrderDate == orderDate);
            const string expectedSql = "select [dbo].[CUSTOMER].[CUSTOMER_ID] from [dbo].[CUSTOMER] join [dbo].[ORDER] on " + 
                                       "([dbo].[CUSTOMER].[CUSTOMER_ID] = [dbo].[ORDER].[CUSTOMER_ID])"
                                       + " where [dbo].[ORDER].[ORDER_DATE] = @p1";

            GeneratedSqlIs(expectedSql);

            Parameter(0).Is(orderDate);

        }
    }
}