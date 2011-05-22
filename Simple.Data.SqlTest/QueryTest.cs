using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class QueryTest
    {
        [Test]
        public void CountWithNoCriteriaShouldSelectThree()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(3, db.Users.Count());
        }

        [Test]
        public void CountWithCriteriaShouldSelectTwo()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(2, db.Users.Count(db.Users.Age > 30));
        }

        [Test]
        public void CountByShouldSelectOne()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(1, db.Users.CountByName("Bob"));
        }

        [Test]
        public void ExistsWithNoCriteriaShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(true, db.Users.Exists());
        }

        [Test]
        public void ExistsWithValidCriteriaShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(true, db.Users.Exists(db.Users.Age > 30));
        }

        [Test]
        public void ExistsWithInvalidCriteriaShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(false, db.Users.Exists(db.Users.Age == -1));
        }

        [Test]
        public void ExistsByValidValueShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(true, db.Users.ExistsByName("Bob"));
        }

        [Test]
        public void ExistsByInvalidValueShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(false, db.Users.ExistsByName("Peter Kay"));
        }

        [Test]
        public void ColumnAliasShouldChangeDynamicPropertyName()
        {
            var db = DatabaseHelper.Open();
            var actual = db.Users.QueryById(1).Select(db.Users.Name.As("Alias")).First();
            Assert.AreEqual("Bob", actual.Alias);
        }

        [Test]
        public void ShouldSelectFromOneToTen()
        {
            var db = DatabaseHelper.Open();
            var query = db.PagingTest.QueryById(1.to(100)).Take(10);
            int index = 1;
            foreach (var row in query)
            {
                Assert.AreEqual(index, row.Id);
                index++;
            }
        }

        [Test]
        public void ShouldSelectFromElevenToTwenty()
        {
            var db = DatabaseHelper.Open();
            var query = db.PagingTest.QueryById(1.to(100)).Skip(10).Take(10);
            int index = 11;
            foreach (var row in query)
            {
                Assert.AreEqual(index, row.Id);
                index++;
            }
        }

        [Test]
        public void ShouldSelectFromOneHundredToNinetyOne()
        {
            var db = DatabaseHelper.Open();
            var query = db.PagingTest.QueryById(1.to(100)).OrderByDescending(db.PagingTest.Id).Skip(0).Take(10);
            int index = 100;
            foreach (var row in query)
            {
                Assert.AreEqual(index, row.Id);
                index--;
            }
        }
    }
}
