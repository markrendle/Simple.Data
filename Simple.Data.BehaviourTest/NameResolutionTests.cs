using System;
using System.Data.Entity.Design.PluralizationServices;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    using System.Globalization;
    using Extensions;


    public class SingularNamesResolutionTests : DatabaseIntegrationContext
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

        [Test]
        public void NaturalJoinWithIndexersCreatesCorrectCommand()
        {
            var orderDate = new DateTime(2010, 1, 1);
            _db["Customer"].Find(_db["Customers"]["Orders"]["OrderDate"] == orderDate);
            
            GeneratedSqlIs("select [dbo].[Customer].[CustomerId] from [dbo].[Customer] join [dbo].[Orders] on ([dbo].[Customer].[CustomerId] = [dbo].[Orders].[CustomerId]) where [dbo].[Orders].[OrderDate] = @p1");
            Parameter(0).Is(orderDate);
        }
    }


    public class PluralNamesResolutionTests : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Customers", "BASE TABLE" },
                                         new[] { "dbo", "Orders", "BASE TABLE" },
                                         new[] { "dbo", "Company", "BASE TABLE"});
            schemaProvider.SetColumns(new[] { "dbo", "Customers", "CustomerId" },
                                          new[] { "dbo", "Orders", "OrderId" },
                                          new[] { "dbo", "Orders", "CustomerId" },
                                          new[] { "dbo", "Orders", "OrderDate" },
                                          new[] { "dbo", "Company", "Id"});
            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Customers", "CustomerId", 0 });
            schemaProvider.SetForeignKeys(new object[] { "FK_Orders_Customers", "dbo", "Orders", "CustomerId", "dbo", "Customers", "CustomerId", 0 });
        }

        [Test]
        public void IndexerMethodWorksWithPluralFromSingular()
        {
            _db["Customer"].All().ToList();
            GeneratedSqlIs("select [dbo].[Customers].[CustomerId] from [dbo].[Customers]");
        }

        [Test]
        public void IndexerMethodWorksWithSchemaAndPluralFromSingular()
        {
            _db["dbo"]["Customer"].All().ToList();
            GeneratedSqlIs("select [dbo].[Customers].[CustomerId] from [dbo].[Customers]");
        }

#if(!MONO)
        [Test]
        public void CompaniesPluralizationIsResolved()
        {
            Database.SetPluralizer(new EntityPluralizer());
            _db.Companies.All().ToList();
            GeneratedSqlIs("select [dbo].[Company].[Id] from [dbo].[Company]");
        }

        class EntityPluralizer : IPluralizer
        {
            private readonly PluralizationService _pluralizationService =
                PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us")); // only English is supported

            public bool IsPlural(string word)
            {
                return _pluralizationService.IsPlural(word);
            }

            public bool IsSingular(string word)
            {
                return _pluralizationService.IsSingular(word);
            }

            public string Pluralize(string word)
            {
                bool upper = (word.IsAllUpperCase());
                word = _pluralizationService.Pluralize(word);
                return upper ? word.ToUpper(_pluralizationService.Culture) : word;
            }

            public string Singularize(string word)
            {
                return _pluralizationService.Singularize(word);
            }
        }
#endif
    }

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
            _db["Customer"].All().ToList();
            GeneratedSqlIs("select [dbo].[CUSTOMER].[CUSTOMER_ID] from [dbo].[CUSTOMER]");
        }

        [Test]
        public void IndexerMethodWorksWithSchemaAndShoutyFromSingular()
        {
            _db["dbo"]["Customer"].All().ToList();
            GeneratedSqlIs("select [dbo].[CUSTOMER].[CUSTOMER_ID] from [dbo].[CUSTOMER]");
        }


        [Test]
        public void NaturalJoinWithShoutyCaseCreatesCorrectCommand()
        {
            var orderDate = new DateTime(2010, 1, 1);
            _db.Customer.Find(_db.Customers.Orders.OrderDate == orderDate);
            const string expectedSql = "select [dbo].[CUSTOMER].[CUSTOMER_ID] from [dbo].[CUSTOMER] join [dbo].[ORDER] on " + 
                                       "([dbo].[CUSTOMER].[CUSTOMER_ID] = [dbo].[ORDER].[CUSTOMER_ID])"
                                     + " where [dbo].[ORDER].[ORDER_DATE] = @p1";

            GeneratedSqlIs(expectedSql);

            Parameter(0).Is(orderDate);

        }
    }
}
