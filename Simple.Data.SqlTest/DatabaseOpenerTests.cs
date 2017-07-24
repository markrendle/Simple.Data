using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Shitty.Data.SqlTest
{
    using Ado;
    using SqlServer;

    [TestFixture]
    public class DatabaseOpenerTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void OpenNamedConnectionTest()
        {
            if (Environment.GetEnvironmentVariable("SIMPLETESTDB") != null)
            {
                Assert.Ignore();
                return;
            }
            var db = Database.OpenNamedConnection("Test");
            Assert.IsNotNull(db);
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestProviderIsSqlProvider()
        {
            var provider = new ProviderHelper().GetProviderByConnectionString(DatabaseHelper.ConnectionString);
            Assert.IsInstanceOf(typeof(SqlConnectionProvider), provider);
        }

        [Test]
        public void TestProviderIsSqlProviderFromOpen()
        {
            Database db = DatabaseHelper.Open();
            Assert.IsInstanceOf(typeof(AdoAdapter), db.GetAdapter());
            Assert.IsInstanceOf(typeof(SqlConnectionProvider), ((AdoAdapter)db.GetAdapter()).ConnectionProvider);
        }

        [Test]
        public void TestProviderIsSqlProviderFromOpenConnection()
        {
            Database db = Database.OpenConnection(DatabaseHelper.ConnectionString);
            Assert.IsInstanceOf(typeof(AdoAdapter), db.GetAdapter());
            Assert.IsInstanceOf(typeof(SqlConnectionProvider), ((AdoAdapter)db.GetAdapter()).ConnectionProvider);
        }
    }
}
