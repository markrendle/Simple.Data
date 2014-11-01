namespace Simple.Data.IntegrationTest
{
    using Mocking.Ado;
    using Xunit;

    public class DeleteTest : DatabaseIntegrationContext
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

        [Fact]
        public async void TestDeleteWithNamedArguments()
        {
            await TargetDb.Users.Delete(Id: 1);
            GeneratedSqlIs("delete from [dbo].[Users] where [dbo].[Users].[Id] = @p1");
            Parameter(0).Is(1);
        }

        [Fact]
        public async void TestDeleteBy()
        {
            await TargetDb.Users.DeleteById(1);
            GeneratedSqlIs("delete from [dbo].[Users] where [dbo].[Users].[Id] = @p1");
            Parameter(0).Is(1);
        }

        [Fact]
        public async void TestDeleteAllWithNoCriteria()
        {
            await TargetDb.Users.DeleteAll();
            GeneratedSqlIs("delete from [dbo].[Users]");
        }

        [Fact]
        public async void TestDeleteAllWithSimpleCriteria()
        {
            await TargetDb.Users.DeleteAll(TargetDb.Users.Age > 42);
            GeneratedSqlIs("delete from [dbo].[Users] where [dbo].[Users].[Age] > @p1");
            Parameter(0).Is(42);
        }

        [Fact]
        public async void TestDeleteAllMoreComplexCriteria()
        {
            await TargetDb.Users.DeleteAll(TargetDb.Users.Age > 42 && TargetDb.Users.Name.Like("J%"));
            GeneratedSqlIs("delete from [dbo].[Users] where ([dbo].[Users].[Age] > @p1 AND [dbo].[Users].[Name] LIKE @p2)");
            Parameter(0).Is(42);
            Parameter(1).Is("J%");
        }

    }
}