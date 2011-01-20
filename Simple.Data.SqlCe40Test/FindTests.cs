using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using System.IO;
using Simple.Data.Ado;
using Simple.Data.SqlCe40;
using Simple.Data.SqlCeTest;

namespace Simple.Data.SqlCe40Test
{
    /// <summary>
    /// Summary description for FindTests
    /// </summary>
    [TestFixture]
    public class FindTests
    {
        private static readonly string DatabasePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)),
            "TestDatabase.sdf");

        [Test]
        public void TestMethod1()
        {
            var provider = ProviderHelper.GetProviderByFilename(DatabasePath);
            Assert.IsInstanceOf(typeof (SqlCe40ConnectionProvider), provider);
        }

        [Test]
        public void TestFindById()
        {
            var db = Database.Opener.OpenFile(DatabasePath);
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestAll()
        {
            var db = Database.OpenFile(DatabasePath);
            var all = new List<object>(db.Users.All().Cast<dynamic>());
            Assert.IsNotEmpty(all);
        }

        [Test]
        public void TestImplicitCast()
        {
            var db = Database.OpenFile(DatabasePath);
            User user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestImplicitEnumerableCast()
        {
            var db = Database.OpenFile(DatabasePath);
            foreach (User user in db.Users.All())
            {
                Assert.IsNotNull(user);
            }
        }

        [Test]
        public void TestInsert()
        {
            var db = Database.OpenFile(DatabasePath);

            var user = db.Users.Insert(Name: "Alice", Password: "foo", Age: 29);

            Assert.IsNotNull(user);
            Assert.AreEqual("Alice", user.Name);
            Assert.AreEqual("foo", user.Password);
            Assert.AreEqual(29, user.Age);
        }
    }
}
