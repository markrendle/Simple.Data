namespace Shitty.Data.IntegrationTest
{
    using System;
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class WhereTest : DatabaseIntegrationContext
    {
        [Test]
        public void WhereWithNoParametersShouldThrowBadExpressionException()
        {
            Assert.Throws<BadExpressionException>(() => _db.Users.All().Where());
        }

        [Test]
        public void WhereWithStringParameterShouldThrowBadExpressionException()
        {
            Assert.Throws<BadExpressionException>(() => _db.Users.All().Where("Answers"));
        }

        [Test]
        public void WhereWithNullParameterShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _db.Users.All().Where(null));
        }

        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" },
                                     new[] { "dbo", "UserBio", "BASE TABLE" },
                                     new[] { "dbo", "UserPayment", "BASE TABLE" },
                                     new[] { "dbo", "Employee", "BASE TABLE" });

            schemaProvider.SetColumns(new object[] { "dbo", "Users", "Id", true },
                                      new[] { "dbo", "Users", "Name" },
                                      new[] { "dbo", "Users", "Password" },
                                      new[] { "dbo", "Users", "Age" },
                                      new[] { "dbo", "UserBio", "UserId" },
                                      new[] { "dbo", "UserBio", "Text" },
                                      new[] { "dbo", "UserPayment", "UserId" },
                                      new[] { "dbo", "UserPayment", "Amount" },
                                      new[] { "dbo", "Employee", "Id" },
                                      new[] { "dbo", "Employee", "Name" },
                                      new[] { "dbo", "Employee", "ManagerId" });

            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Users", "Id", 0 });
            schemaProvider.SetForeignKeys(new object[] { "FK_Users_UserBio", "dbo", "UserBio", "UserId", "dbo", "Users", "Id", 0 },
                                          new object[] { "FK_Users_UserPayment", "dbo", "UserPayment", "UserId", "dbo", "Users", "Id", 0 });
        }
    }
}