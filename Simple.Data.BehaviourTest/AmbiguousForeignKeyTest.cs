namespace Shitty.Data.IntegrationTest
{
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class AmbiguousForeignKeyTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Fixture", "BASE TABLE" },
                                         new[] { "dbo", "Team", "BASE TABLE" });
            schemaProvider.SetColumns(new[] { "dbo", "Team", "Id" },
                                          new[] { "dbo", "Fixture", "Id" },
                                          new[] { "dbo", "Fixture", "HomeTeamId" },
                                          new[] { "dbo", "Fixture", "AwayTeamId" },
                                          new[] { "dbo", "Team", "Name" });
            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Team", "Id", 0 },
                new object[] { "dbo", "Team", "Id", 0 });
            schemaProvider.SetForeignKeys(new object[] { "FK_Fixture_HomeTeam", "dbo", "Fixture", "HomeTeamId", "dbo", "Team", "Id", 0 },
                new object[] { "FK_Fixture_AwayTeam", "dbo", "Fixture", "AwayTeamId", "dbo", "Team", "Id", 0 });
        }

        [Test]
        public void CanSelectTwoWithUsingExplicitJoin()
        {
            const string expectedSql =
                "select [dbo].[Fixture].[Id],[dbo].[Fixture].[HomeTeamId],[dbo].[Fixture].[AwayTeamId],[Home].[Id] AS [__withn__Home__Id],[Home].[Name] AS [__withn__Home__Name]," +
                "[Away].[Id] AS [__withn__Away__Id],[Away].[Name] AS [__withn__Away__Name] from [dbo].[Fixture] " +
                "JOIN [dbo].[Team] [Home] ON ([dbo].[Fixture].[HomeTeamId] = [Home].[Id]) " +
                "JOIN [dbo].[Team] [Away] ON ([dbo].[Fixture].[AwayTeamId] = [Away].[Id])";

            dynamic homeTeam;
            dynamic awayTeam;

            var q = _db.Fixture.All()
                .Join(_db.Team.As("Home"), out homeTeam)
                .On(_db.Fixture.HomeTeamId == homeTeam.Id)
                .Join(_db.Team.As("Away"), out awayTeam)
                .On(_db.Fixture.AwayTeamId == awayTeam.Id)
                .With(homeTeam)
                .With(awayTeam);

            EatException(() => q.ToList());
            GeneratedSqlIs(expectedSql);
        }
    }
}
