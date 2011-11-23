using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.IntegrationTest.Query
{
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class FunctionTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" });

            schemaProvider.SetColumns(new[] { "dbo", "Users", "Name" },
                                      new[] { "dbo", "Users", "Password" });
        }

        private const string usersColumns = "[dbo].[Users].[Name], [dbo].[Users].[Password]";

        [Test]
        public void SubstringIsEnteredCorrectlyInFindAll()
        {
            const string expected = @"select [dbo].[users].[name],[dbo].[users].[password] from [dbo].[users] where substring([dbo].[users].[name],@p1,@p2) = @p3";

            EatException<InvalidOperationException>(() =>
                _db.Users.FindAll(_db.Users.Name.Substring(0, 1) == "A").ToList());

            GeneratedSqlIs(expected);
            Parameter(0).Is(0);
            Parameter(1).Is(1);
            Parameter(2).Is("A");
        }

        [Test]
        public void SubstringIsEnteredCorrectlyInFindOne()
        {
            const string expected = @"select " + usersColumns + " from [dbo].[users] where substring([dbo].[users].[name],@p1,@p2) = @p3";

            _db.Users.Find(_db.Users.Name.Substring(0, 1) == "A");

            GeneratedSqlIs(expected);
            Parameter(0).Is(0);
            Parameter(1).Is(1);
            Parameter(2).Is("A");
        }
    }
}
