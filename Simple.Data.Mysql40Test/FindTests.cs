using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using System.IO;
using Simple.Data.Ado;

namespace Simple.Data.Mysql40Test
{
    /// <summary>
    /// Summary description for FindTests
    /// </summary>
    [TestFixture]
    public class FindTests
    {
        private static readonly string ConnectionString =
           "server=localhost;user=SimpleData;database=SimpleDataTest;password=test;";

        [TestFixtureSetUp]
        public void DeleteAlice()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);
            db.Users.DeleteByName("Alice");
        }

        [Test]
        public void TestProviderWithConnectionString()
        {
            var provider = ProviderHelper.GetProviderByConnectionString(ConnectionString);
            Assert.IsInstanceOf(typeof(Mysql40.Mysql40ConnectionProvider), provider);
        }

        [Test]
        public void TestFindById()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestAll()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);
            var all = new List<object>(db.Users.All().Cast<dynamic>());
            Assert.IsNotEmpty(all);
        }

        [Test]
        public void TestImplicitCast()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);
            User user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestImplicitEnumerableCast()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);
            foreach (User user in db.Users.All())
            {
                Assert.IsNotNull(user);
            }
        }

        [Test]
        public void TestInsert()
        {
            var db = Database.Opener.OpenConnection(ConnectionString);

            var user = db.Users.Insert(Name: "Alice", Password: "foo", Age: 29);

            Assert.IsNotNull(user);
            Assert.AreEqual("Alice", user.Name);
            Assert.AreEqual("foo", user.Password);
            Assert.AreEqual(29, user.Age);
        }
    }
}
