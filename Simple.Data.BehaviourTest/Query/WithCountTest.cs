using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.IntegrationTest.Query
{
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class WithCountTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" });

            schemaProvider.SetColumns(new[] { "dbo", "Users", "Name" },
                                      new[] { "dbo", "Users", "Password" });
        }

        [Test]
        public void WithTotalCountShouldCreateCompoundQuery()
        {
            const string expected = @"select count(*) from [dbo].[users] where [dbo].[users].[name] = @p1_c0; " +
                @"select [dbo].[users].[name],[dbo].[users].[password] from [dbo].[users] where [dbo].[users].[name] = @p1_c1";

            Future<int> count;
            var q = _db.Users.QueryByName("Foo")
                .WithTotalCount(out count);

            EatException<InvalidOperationException>(() => q.ToList());

            GeneratedSqlIs(expected);
        }
    }
}
