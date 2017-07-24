using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shitty.Data.Ado;
using Shitty.Data.Mocking;

namespace Shitty.Data.IntegrationTest.Query
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
// ReSharper disable InconsistentNaming
        public void WithTotalCountShouldCreateCompoundQuery_ObsoleteFutureVersion()
// ReSharper restore InconsistentNaming
        {
            const string expected = @"select count(*) from [dbo].[users] where [dbo].[users].[name] = @p1_c0; " +
                @"select [dbo].[users].[name],[dbo].[users].[password] from [dbo].[users] where [dbo].[users].[name] = @p1_c1";

            Future<int> count;
            var q = _db.Users.QueryByName("Foo")
                .WithTotalCount(out count);

            EatException<InvalidOperationException>(() => q.ToList());

            GeneratedSqlIs(expected);
        }

        [Test]
        public void WithTotalCountShouldCreateCompoundQuery()
        {
            const string expected = @"select count(*) from [dbo].[users] where [dbo].[users].[name] = @p1_c0; " +
                @"select [dbo].[users].[name],[dbo].[users].[password] from [dbo].[users] where [dbo].[users].[name] = @p1_c1";

            Promise<int> count;
            var q = _db.Users.QueryByName("Foo")
                .WithTotalCount(out count);

            EatException<InvalidOperationException>(() => q.ToList());

            GeneratedSqlIs(expected);
        }

        [Test]
        public void WithTotalCountWithExplicitSelectShouldCreateCompoundQuery()
        {
            const string expected = @"select count(*) from [dbo].[users] where [dbo].[users].[name] = @p1_c0; " +
                @"select [dbo].[users].[name] from [dbo].[users] where [dbo].[users].[name] = @p1_c1";

            Promise<int> count;
            var q = _db.Users.QueryByName("Foo")
                .Select(_db.Users.Name)
                .WithTotalCount(out count);

            EatException<InvalidOperationException>(() => q.ToList());

            GeneratedSqlIs(expected);
        }
    }
}
