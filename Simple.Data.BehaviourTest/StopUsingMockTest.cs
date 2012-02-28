namespace Simple.Data.IntegrationTest
{
    using Ado;
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class StopUsingMockTest : DatabaseIntegrationContext
    {
        [Test]
        public void StopUsingMockAdapterStopsUsingMockAdapter()
        {
            var mock = new InMemoryAdapter();
            Database.UseMockAdapter(mock);
            Database db = Database.OpenNamedConnection("Mock");
            Assert.AreSame(mock, db.GetAdapter());
            Database.StopUsingMockAdapter();
            db = Database.OpenNamedConnection("Mock");
            Assert.IsInstanceOf<AdoAdapter>(db.GetAdapter());
        }

        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
        }
    }
}
