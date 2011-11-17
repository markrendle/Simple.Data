namespace Simple.Data.UnitTest
{
    using NUnit.Framework;

    [TestFixture]
    public class DatabaseOpenerTest
    {
        [Test]
        public void UseMockDatabaseReturnsMockDatabaseOnOpen()
        {
            var database = new Database(null);
            DatabaseOpener.UseMockDatabase(database);

            Assert.AreEqual(database, Database.Open());
        }

        [Test]
        public void UseMockDatabaseReturnsMockDatabaseOnOpenConnection()
        {
            var database = new Database(null);
            DatabaseOpener.UseMockDatabase(database);

            Assert.AreEqual(database, Database.OpenConnection("data source=."));
        }

        [Test]
        public void UseMockDatabaseReturnsMockDatabaseOnOpenNamedConnection()
        {
            var database = new Database(null);
            DatabaseOpener.UseMockDatabase(database);

            Assert.AreEqual(database, Database.OpenNamedConnection("Steve"));
        }

        [Test]
        public void UseMockDatabaseReturnsMockDatabaseOnOpenFile()
        {
            var database = new Database(null);
            DatabaseOpener.UseMockDatabase(database);

            Assert.AreEqual(database, Database.OpenFile("any.sdb"));
        }

        [Test]
        public void UseMockDatabaseWithFuncReturnsMockDatabaseOnOpen()
        {
            var database = new Database(null);
            DatabaseOpener.UseMockDatabase(() => database);

            Assert.AreEqual(database, Database.Open());
        }

        [Test]
        public void UseMockDatabaseWithFuncReturnsMockDatabaseOnOpenConnection()
        {
            var database = new Database(null);
            DatabaseOpener.UseMockDatabase(() => database);

            Assert.AreEqual(database, Database.OpenConnection("data source=."));
        }

        [Test]
        public void UseMockDatabaseWithFuncReturnsMockDatabaseOnOpenNamedConnection()
        {
            var database = new Database(null);
            DatabaseOpener.UseMockDatabase(() => database);

            Assert.AreEqual(database, Database.OpenNamedConnection("Steve"));
        }

        [Test]
        public void UseMockDatabaseWithFuncReturnsMockDatabaseOnOpenFile()
        {
            var database = new Database(null);
            DatabaseOpener.UseMockDatabase(() => database);

            Assert.AreEqual(database, Database.OpenFile("any.sdb"));
        }
    }
}
