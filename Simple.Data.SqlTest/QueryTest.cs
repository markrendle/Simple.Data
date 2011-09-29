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
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void CountWithNoCriteriaShouldSelectThree()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(3, db.Users.GetCount());
        }

        [Test]
        public void CountWithCriteriaShouldSelectTwo()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(2, db.Users.GetCount(db.Users.Age > 30));
        }

        [Test]
        public void CountByShouldSelectOne()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(1, db.Users.GetCountByName("Bob"));
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
        public void ExistsWithInvalidCriteriaShouldReturnFalse()
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
        public void ExistsByInvalidValueShouldReturnFalse()
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

        [Test]
        public void WithTotalCountShouldGiveCount()
        {
            Promise<int> count;
            var db = DatabaseHelper.Open();
            var list = db.PagingTest.QueryById(1.to(50))
                .Take(10)
                .WithTotalCount(out count)
                .ToList();

            Assert.AreEqual(10, list.Count);
            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(50, count);
        }

        [Test]
        public void WithTotalCountWithExplicitSelectShouldGiveCount()
        {
            Promise<int> count;
            var db = DatabaseHelper.Open();
            List<dynamic> list = db.PagingTest.QueryById(1.to(50))
                .Select(db.PagingTest.Id)
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(50, count);
            Assert.AreEqual(10, list.Count);
            foreach (var dictionary in list.Cast<IDictionary<string,object>>())
            {
                Assert.AreEqual(1, dictionary.Count);
            }
        }

        [Test]
        public void WithTotalCountWithExplicitSelectAndOrderByShouldGiveCount()
        {
            Promise<int> count;
            var db = DatabaseHelper.Open();
            List<dynamic> list = db.PagingTest.QueryById(1.to(50))
                .Select(db.PagingTest.Id)
                .OrderByDescending(db.PagingTest.Id)
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(50, count);
            Assert.AreEqual(10, list.Count);
            foreach (var dictionary in list.Cast<IDictionary<string, object>>())
            {
                Assert.AreEqual(1, dictionary.Count);
            }
        }

        [Test]
        public void WithTotalCountShouldGiveCount_ObsoleteFutureVersion()
        {
            Future<int> count;
            var db = DatabaseHelper.Open();
            var list = db.PagingTest.QueryById(1.to(50))
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            Assert.AreEqual(10, list.Count);
            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(50, count);
        }

        [Test]
        public void ShouldDirectlyQueryDetailTable()
        {
            var db = DatabaseHelper.Open();
            var order = db.Customers.QueryByNameAndAddress("Test", "100 Road").Orders.FirstOrDefault();
            Assert.IsNotNull(order);
            Assert.AreEqual(1, order.OrderId);
        }

        [Test]
        public void ShouldReturnNullWhenNoRowFound()
        {
            var db = DatabaseHelper.Open();
            string name = db.Customers
                        .Query()
                        .Select(db.Customers.Name)
                        .Where(db.Customers.CustomerId == 0) // There is no CustomerId 0
                        .OrderByName()
                        .Take(1) // Should return only one record no matter what
                        .ToScalarOrDefault<string>();
            Assert.IsNull(name);
        }

        [Test]
        public void ToScalarListShouldReturnStringList()
        {
            var db = DatabaseHelper.Open();
            List<string> name = db.Customers
                        .Query()
                        .Select(db.Customers.Name)
                        .OrderByName()
                        .ToScalarList<string>();
            Assert.IsNotNull(name);
            Assert.AreNotEqual(0, name.Count);
        }

        [Test]
        public void ToScalarArrayShouldReturnStringArray()
        {
            var db = DatabaseHelper.Open();
            string[] name = db.Customers
                        .Query()
                        .Select(db.Customers.Name)
                        .OrderByName()
                        .ToScalarArray<string>();
            Assert.IsNotNull(name);
            Assert.AreNotEqual(0, name.Length);
        }

        [Test]
        public void HavingWithMinDateShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row =
                db.GroupTestMaster.Query().Having(db.GroupTestMaster.GroupTestDetail.Date.Min() >=
                                                  new DateTime(2000, 1, 1))
                                                  .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("Two", row.Name);
        }
        [Test]
        public void HavingWithMaxDateShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row =
                db.GroupTestMaster.Query().Having(db.GroupTestMaster.GroupTestDetail.Date.Max() <
                                                  new DateTime(2009, 1, 1))
                                                  .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("One", row.Name);
        }

        [Test]
        public void HavingWithCountShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row = db.GroupTestMaster.Query()
                .Having(db.GroupTestMaster.GroupTestDetail.Id.Count() == 2)
                .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("Two", row.Name);
        }

        [Test]
        public void HavingWithAverageShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row = db.GroupTestMaster.Query()
                .Having(db.GroupTestMaster.GroupTestDetail.Number.Average() == 2)
                .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("One", row.Name);
        }

        [Test]
        public void ToScalarOrDefault()
        {
            var db = DatabaseHelper.Open();
            int max = db.Users.FindAllByName("ZXCVBNM").Select(db.Users.Age.Max()).ToScalarOrDefault<int>();
            Assert.AreEqual(0, max);
        }
    }
}
