using System;
using NUnit.Framework;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    
    [TestFixture]
    public class ExceptionsTesting : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" });
            schemaProvider.SetColumns(new object[] { "dbo", "Users", "Id", true },
                                            new[] { "dbo", "Users", "Name" },
                                            new[] { "dbo", "Users", "Password" },
                                            new[] { "dbo", "Users", "Age" });
            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Users", "Id", 0 });
        }

        [Test]
        public void Unknown_table_raises_exception()
        {
            var x = Assert.Throws<UnresolvableObjectException>(() => _db.People.All().ToList<dynamic>());
            Assert.AreEqual("dbo.People", x.ObjectName);
        }

        [Test]
        public void Unknown_column_raises_exception()
        {
            var x = Assert.Throws<UnresolvableObjectException>(() => _db.Users.Find(_db.Users.FirstName == "Joe").ToList<dynamic>());
            Assert.AreEqual("Users.FirstName", x.ObjectName);
        }

        [Test]
        public void Unknown_column_by_method_raises_exception()
        {
            var x = Assert.Throws<UnresolvableObjectException>(() => _db.Users.FindByFirstName("Joe").ToList<dynamic>());
            Assert.AreEqual("Users.FirstName", x.ObjectName);
        }


        [Test]
        public void Unknown_column_on_order_by_raises_exception()
        {
            var x = Assert.Throws<UnresolvableObjectException>(() => _db.Users.FindAll(_db.Users.Name == "Joe").OrderByFirstName().ToList<dynamic>());
            Assert.AreEqual("Users.FirstName", x.ObjectName);
        }
    }
}