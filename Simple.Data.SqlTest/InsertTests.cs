using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class InsertTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestInsertWithNamedArguments()
        {
            var db = DatabaseHelper.Open();

            var user = db.Users.Insert(Name: "Ford", Password: "hoopy", Age: 29);

            Assert.IsNotNull(user);
            Assert.AreEqual("Ford", user.Name);
            Assert.AreEqual("hoopy", user.Password);
            Assert.AreEqual(29, user.Age);
        }

        [Test]
        public void TestInsertWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var user = new User {Name = "Zaphod", Password = "zarquon", Age = 42};

            var actual = db.Users.Insert(user);

            Assert.IsNotNull(user);
            Assert.AreEqual("Zaphod", actual.Name);
            Assert.AreEqual("zarquon", actual.Password);
            Assert.AreEqual(42, actual.Age);
        }

        [Test]
        public void TestInsertWithDynamicTypeObject()
        {
            var db = DatabaseHelper.Open();

            dynamic user = new ExpandoObject();
            user.Name = "Marvin";
            user.Password = "diodes";
            user.Age = 42000000;

            var actual = db.Users.Insert(user);

            Assert.IsNotNull(user);
            Assert.AreEqual("Marvin", actual.Name);
            Assert.AreEqual("diodes", actual.Password);
            Assert.AreEqual(42000000, actual.Age);
        }
    }
}
