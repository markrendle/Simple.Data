using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.MongoDbTest
{
    [TestFixture]
    public class UpdateTests
    {
        [SetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestUpdateWithNamedArguments()
        {
            var db = DatabaseHelper.Open();

            int recordsAffected = db.Users.UpdateById(Id: 1, Name: "Ford");
            Assert.AreEqual(1, recordsAffected);
        }

        [Test]
        public void TestInsertWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            User user = db.Users.FindById(2);
            user.Name = "Ford";

            int recordsAffected = db.Users.Update(user);
            Assert.AreEqual(1, recordsAffected);
        }

        [Test]
        public void TestInsertWithDynamicTypeObject()
        {
            var db = DatabaseHelper.Open();

            var user = db.Users.FindById(2);
            user.Name = "Ford";

            int recordsAffected = db.Users.Update(user);
            Assert.AreEqual(1, recordsAffected);
        }
    }
}
