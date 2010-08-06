using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.SqlCe;
using System.IO;

namespace Simple.Data.SqlCeTest
{
    /// <summary>
    /// Summary description for FindTests
    /// </summary>
    [TestClass]
    public class FindTests
    {
        private static readonly string DatabasePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)),
            "TestDatabase.sdf");

        [TestMethod]
        public void TestMethod1()
        {
            var provider = ProviderHelper.GetProviderByFilename(DatabasePath);
            Assert.IsInstanceOfType(provider, typeof(SqlCeProvider));
        }

        [TestMethod]
        public void TestFindById()
        {
            var db = Database.OpenFile(DatabasePath);
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [TestMethod]
        public void TestAllCount()
        {
            var db = Database.OpenFile(DatabasePath);
            var count = db.Users.All.Count;
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void TestImplicitCast()
        {
            var db = Database.OpenFile(DatabasePath);
            User user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [TestMethod]
        public void TestImplicitEnumerableCast()
        {
            var db = Database.OpenFile(DatabasePath);
            foreach (User user in db.Users.All)
            {
                Assert.IsNotNull(user);
            }
        }

        [TestMethod]
        public void TestInsert()
        {
            var db = Database.OpenFile(DatabasePath);

            db.Users.Insert(Name: "Alice", Password: "foo", Age: 29);
            User user = db.Users.FindByNameAndPassword("Alice", "foo");

            Assert.IsNotNull(user);
            Assert.AreEqual(29, user.Age);
        }
    }
}
