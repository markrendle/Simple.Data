using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Simple.Data.MongoDb;

namespace Simple.Data.MongoDbTest
{
    /// <summary>
    /// Summary description for FindTests
    /// </summary>
    [TestFixture]
    public class FindTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestFindById()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }

        [Test]
        public void TestFindAllByName()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAllByName("Bob").Cast<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Test]
        public void TestAnd()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Age > 32 & db.Users.Name == "Dave").Cast<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Test]
        public void TestGreaterThan()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Age > 32).Cast<User>();
            Assert.AreEqual(2, users.Count());
        }

        [Test]
        public void TestGreaterThanOrEqual()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Age >= 32).Cast<User>();
            Assert.AreEqual(3, users.Count());
        }

        [Test]
        public void TestLessThan()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Age < 49).Cast<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Test]
        public void TestLessThanOrEqual()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Age <= 49).Cast<User>();
            Assert.AreEqual(3, users.Count());
        }

        [Test]
        public void TestLike()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Name.Like("Bob")).ToList<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Test]
        public void TestNotEqual()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Age != 32).ToList<User>();
            Assert.AreEqual(2, users.Count());
        }

        [Test]
        public void TestOr()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Age == 32 | db.Users.Name == "Dave").Cast<User>();
            Assert.AreEqual(2, users.Count());
        }

        [Test]
        public void TestRange()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAllByAge(32.to(48)).Cast<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Test]
        public void TestStartsWith()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Name.StartsWith("D")).Cast<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Test]
        public void TestContains()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Name.Contains("a")).Cast<User>();
            Assert.AreEqual(2, users.Count());
        }

        [Test]
        public void TestEndsWith()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Name.EndsWith("b")).Cast<User>();
            Assert.AreEqual(1, users.Count());
        }

        [Test]
        public void TestNestedDocuments()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<User> users = db.Users.FindAll(db.Users.Address.State == "TX").Cast<User>();
            Assert.AreEqual(2, users.Count());
        }

        [Test]
        public void TestAllCount()
        {
            var db = DatabaseHelper.Open();
            var count = db.Users.All().ToList().Count;
            Assert.AreEqual(3, count);
        }

        [Test]
        public void TestDynamicUsage()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("Jane", user.Dependents.First().Name);
            Assert.AreEqual("b@b.com", user.EmailAddresses[1]);
            Assert.AreEqual("123 Way", user.Address.Line);
        }

        [Test]
        public void TestImplicitCastOfList()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.FindById(1);
            IEnumerable<string> emails = user.EmailAddresses;
            Assert.AreEqual(2, emails.Count());
            Assert.AreEqual("b@b.com", emails.ElementAt(1));
        }

        [Test]
        public void TestImplicitCast()
        {
            var db = DatabaseHelper.Open();
            User user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("Jane", user.Dependents.First().Name);
            Assert.AreEqual("b@b.com", user.EmailAddresses[1]);
            Assert.AreEqual("123 Way", user.Address.Line);
        }

        [Test]
        public void TestImplicitEnumerableCast()
        {
            var db = DatabaseHelper.Open();
            foreach (User user in db.Users.All())
            {
                Assert.IsNotNull(user);
            }
        }
    }
}