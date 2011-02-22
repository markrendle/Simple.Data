using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.MongoDbTest
{
    [TestFixture]
    public class DeleteTests
    {
        [SetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestDeleteById()
        {
            var db = DatabaseHelper.Open();

            int recordsAffected = db.Users.DeleteById(2);
            Assert.AreEqual(1, recordsAffected);
        }

        [Test]
        public void TestDeleteWithParameters()
        {
            var db = DatabaseHelper.Open();

            int recordsAffected = db.Users.Delete(Age: 49);
            Assert.AreEqual(2, recordsAffected);
        }
    }
}