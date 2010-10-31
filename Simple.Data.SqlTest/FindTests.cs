using System.Data.SqlClient;
using System.Reflection;
using NUnit.Framework;
using System.IO;
using Simple.Data.Ado;

namespace Simple.Data.SqlTest
{
    /// <summary>
    /// Summary description for FindTests
    /// </summary>
    [TestFixture]
    public class FindTests
    {
        [Test]
        public void TestMethod1()
        {
            var provider = ProviderHelper.GetProviderByConnectionString("data source=.");
            Assert.IsInstanceOf(typeof(SqlConnectionProvider), provider);
        }

        [Test]
        public void TestFindById()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        [Ignore] // This won't work until the database gets reset before every run.
        public void TestAllCount()
        {
            var db = DatabaseHelper.Open();
            var count = db.Users.All.Count;
            Assert.AreEqual(3, count);
        }

        [Test]
        public void TestImplicitCast()
        {
            var db = DatabaseHelper.Open();
            User user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestImplicitEnumerableCast()
        {
            var db = DatabaseHelper.Open();
            foreach (User user in db.Users.All)
            {
                Assert.IsNotNull(user);
            }
        }

        [Test]
        public void TestInsert()
        {
            var db = DatabaseHelper.Open();

            var user = db.Users.Insert(Name: "Alice", Password: "foo", Age: 29);

            Assert.IsNotNull(user);
            Assert.AreEqual("Alice", user.Name);
            Assert.AreEqual("foo", user.Password);
            Assert.AreEqual(29, user.Age);
        }
    }
}
