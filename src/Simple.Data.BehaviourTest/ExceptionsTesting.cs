namespace Simple.Data.IntegrationTest
{
    using Mocking.Ado;
    using Xunit;

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

        [Fact]
        public async void Unknown_table_raises_exception()
        {
            var x = await Assert.ThrowsAsync<UnresolvableObjectException>(() => TargetDb.People.All().ToList<dynamic>());
            Assert.Equal("dbo.People", x.ObjectName);
        }

        [Fact]
        public async void Unknown_column_raises_exception()
        {
            var x = await Assert.ThrowsAsync<UnresolvableObjectException>(() => TargetDb.Users.Find(TargetDb.Users.FirstName == "Joe").ToList<dynamic>());
            Assert.Equal("Users.FirstName", x.ObjectName);
        }

        [Fact]
        public async void Unknown_column_by_method_raises_exception()
        {
            var x = await Assert.ThrowsAsync<UnresolvableObjectException>(() => TargetDb.Users.FindByFirstName("Joe").ToList<dynamic>());
            Assert.Equal("Users.FirstName", x.ObjectName);
        }


        [Fact]
        public async void Unknown_column_on_order_by_raises_exception()
        {
            var x = await Assert.ThrowsAsync<UnresolvableObjectException>(() => TargetDb.Users.FindAll(TargetDb.Users.Name == "Joe").OrderByFirstName().ToList<dynamic>());
            Assert.Equal("Users.FirstName", x.ObjectName);
        }
    }
}