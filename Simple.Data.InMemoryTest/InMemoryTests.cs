using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Simple.Data.InMemoryTest
{
    using InMemory;
    using NUnit.Framework;

    [TestFixture]
    public class InMemoryTests
    {
        [Test]
        public void InsertAndFindShouldWork()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Name: "Alice");
            var record = db.Test.FindById(1);
            Assert.IsNotNull(record);
            Assert.AreEqual(1, record.Id);
            Assert.AreEqual("Alice", record.Name);
        }

        [Test]
        public void InsertAndFindWithTwoColumnsShouldWork()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Name: "Alice");
            var record = db.Test.FindByIdAndName(1, "Alice");
            Assert.IsNotNull(record);
            Assert.AreEqual(1, record.Id);
            Assert.AreEqual("Alice", record.Name);
        }

        [Test]
        public void AllShouldReturnAllRecords()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Name: "Alice");
            db.Test.Insert(Id: 2, Name: "Bob");
            List<dynamic> records = db.Test.All().ToList();
            Assert.IsNotNull(records);
            Assert.AreEqual(2, records.Count);
        }

        [Test]
        public void ShouldWorkWithByteArrays()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Data: new byte[] {0x1, 0x2, 0x3});
            var record = db.Test.FindById(1);
            Assert.AreEqual(0x1, record.Data[0]);
        }

        [Test]
        public void TestUpdateBy()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Name: "Alice");
            int updated = db.Test.UpdateById(Id: 1, Name: "Allyce");
            Assert.AreEqual(1, updated);
            var record = db.Test.FindById(1);
            Assert.AreEqual("Allyce", record.Name);
        }

        [Test]
        public void TestDeleteBy()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Name: "Alice");
            Assert.AreEqual(1, db.Test.All().ToList().Count);
            int deleted = db.Test.DeleteById(1);
            Assert.AreEqual(1, deleted);
            var record = db.Test.FindById(1);
            Assert.IsNull(record);
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [Test]
        public void SeparateThreads_Should_SeeDifferentMocks()
        {
            int r1 = 0;
            int r2 = 0;

            var t1 = new Thread(() => r1 = ThreadTestHelper(1));
            var t2 = new Thread(() => r2 = ThreadTestHelper(2));
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            Assert.AreEqual(1, r1);
            Assert.AreEqual(2, r2);
        }

        private static int ThreadTestHelper(int userId)
        {
            var mockAdapter = new InMemoryAdapter();
            Database.UseMockAdapter(mockAdapter);
            var db = Database.Open();
            db.Users.Insert(Id: userId, Email: "foo");
            return Database.Default.Users.FindByEmail("foo").Id;
        }
    }
}
