namespace Simple.Data.IntegrationTest
{
    using Mocking.Ado;
    using Xunit;

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

        [Fact]
        public async void CanSelectTwoWithUsingExplicitJoin()
        {
            const string expectedSql =
                "select [dbo].[Fixture].[Id],[dbo].[Fixture].[HomeTeamId],[dbo].[Fixture].[AwayTeamId],[Home].[Id] AS [__withn__Home__Id],[Home].[Name] AS [__withn__Home__Name]," +
                "[Away].[Id] AS [__withn__Away__Id],[Away].[Name] AS [__withn__Away__Name] from [dbo].[Fixture] " +
                "JOIN [dbo].[Team] [Home] ON ([dbo].[Fixture].[HomeTeamId] = [Home].[Id]) " +
                "JOIN [dbo].[Team] [Away] ON ([dbo].[Fixture].[AwayTeamId] = [Away].[Id])";

            dynamic homeTeam;
            dynamic awayTeam;

            var q = TargetDb.Fixture.All()
                .Join(TargetDb.Team.As("Home"), out homeTeam)
                .On(TargetDb.Fixture.HomeTeamId == homeTeam.Id)
                .Join(TargetDb.Team.As("Away"), out awayTeam)
                .On(TargetDb.Fixture.AwayTeamId == awayTeam.Id)
                .With(homeTeam)
                .With(awayTeam);

            EatException(async () => await q.ToList());
            GeneratedSqlIs(expectedSql);
        }
    }
}
