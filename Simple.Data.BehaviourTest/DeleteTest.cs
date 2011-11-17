namespace Simple.Data.IntegrationTest
{
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
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

        [Test]
        public void TestDeleteWithNamedArguments()
        {
            _db.Users.Delete(Id: 1);
            GeneratedSqlIs("delete from [dbo].[Users] where [dbo].[Users].[Id] = @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestDeleteBy()
        {
            _db.Users.DeleteById(1);
            GeneratedSqlIs("delete from [dbo].[Users] where [dbo].[Users].[Id] = @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestDeleteAllWithNoCriteria()
        {
            _db.Users.DeleteAll();
            GeneratedSqlIs("delete from [dbo].[Users]");
        }

        [Test]
        public void TestDeleteAllWithSimpleCriteria()
        {
            _db.Users.DeleteAll(_db.Users.Age > 42);
            GeneratedSqlIs("delete from [dbo].[Users] where [dbo].[Users].[Age] > @p1");
            Parameter(0).Is(42);
        }

        [Test]
        public void TestDeleteAllMoreComplexCriteria()
        {
            _db.Users.DeleteAll(_db.Users.Age > 42 && _db.Users.Name.Like("J%"));
            GeneratedSqlIs("delete from [dbo].[Users] where ([dbo].[Users].[Age] > @p1 AND [dbo].[Users].[Name] LIKE @p2)");
            Parameter(0).Is(42);
            Parameter(1).Is("J%");
        }

    }
}