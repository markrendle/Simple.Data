namespace Simple.Data.IntegrationTest.Query
{
    using System;
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class DistinctTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" });

            schemaProvider.SetColumns(new[] { "dbo", "Users", "Name" },
                                      new[] { "dbo", "Users", "Password" });
        }

        [Test]
        public void DistinctShouldAddSelectDistinct()
        {
            const string expected = @"select distinct [dbo].[users].[name],[dbo].[users].[password] from [dbo].[users] where [dbo].[users].[name] = @p1";

            var q = TargetDb.Users.QueryByName("Foo")
                .Distinct();

            EatException<InvalidOperationException>(() => q.ToList());

            GeneratedSqlIs(expected);
        }

        [Test]
        public void NoDistinctShouldNotAddSelectDistinct()
        {
            const string expected = @"select [dbo].[users].[name],[dbo].[users].[password] from [dbo].[users] where [dbo].[users].[name] = @p1";

            var q = TargetDb.Users.QueryByName("Foo");

            EatException<InvalidOperationException>(() => q.ToList());

            GeneratedSqlIs(expected);
        }
    }
}