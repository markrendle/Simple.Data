namespace Simple.Data.InMemoryTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public class InMemoryTests
    {
        [Test]
        public void InsertAndGetShouldWork()
        {
var adapter = new InMemoryAdapter();
adapter.SetKeyColumn("Test", "Id");
Database.UseMockAdapter(adapter);
var db = Database.Open();
db.Test.Insert(Id: 1, Name: "Alice");
var record = db.Test.Get(1);
Assert.IsNotNull(record);
Assert.AreEqual(1, record.Id);
Assert.AreEqual("Alice", record.Name);
        }

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
        public void SelectShouldReturnSubsetOfColumns()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Name: "Alice");
            db.Test.Insert(Id: 2, Name: "Bob");
            List<IDictionary<string, object>> records = db.Test.All().Select(db.Test.Name).ToList<IDictionary<string, object>>();
            Assert.IsNotNull(records);
            Assert.AreEqual(2, records.Count);
            Assert.False(records[0].ContainsKey("Id"));
            Assert.True(records[0].ContainsKey("Name"));
            Assert.False(records[1].ContainsKey("Id"));
            Assert.True(records[1].ContainsKey("Name"));
        }

        [Test]
        public void SelectWithAverageShouldReturnAverage()
        {
            var db = CreateAggregateTestDb();
            var records = db.Test.All().Select(db.Test.Name, db.Test.Age.Average().As("AverageAge")).ToList();
            Assert.AreEqual(2, records.Count);
            Assert.AreEqual(25, records[0].AverageAge);
            Assert.AreEqual(45, records[1].AverageAge);
        }

        [Test]
        public void SelectWithSumShouldReturnSum()
        {
            var db = CreateAggregateTestDb();
            var records = db.Test.All().Select(db.Test.Name, db.Test.Age.Sum().As("SumAge")).ToList();
            Assert.AreEqual(2, records.Count);
            Assert.AreEqual(50, records[0].SumAge);
            Assert.AreEqual(90, records[1].SumAge);
        }

        [Test]
        public void SelectWithMinShouldReturnMin()
        {
            var db = CreateAggregateTestDb();
            var records = db.Test.All().Select(db.Test.Name, db.Test.Age.Min().As("MinAge")).ToList();
            Assert.AreEqual(2, records.Count);
            Assert.AreEqual(20, records[0].MinAge);
            Assert.AreEqual(40, records[1].MinAge);
        }

        [Test]
        public void SelectWithMaxShouldReturnMax()
        {
            var db = CreateAggregateTestDb();
            var records = db.Test.All().Select(db.Test.Name, db.Test.Age.Max().As("MaxAge")).ToList();
            Assert.AreEqual(2, records.Count);
            Assert.AreEqual(30, records[0].MaxAge);
            Assert.AreEqual(50, records[1].MaxAge);
        }

        [Test]
        public void SelectWithHavingSumShouldReturnOnlyMatchingRows()
        {
            var db = CreateAggregateTestDb();
            var records = db.Test.All().Select(db.Test.Name).Having(db.Test.Age.Sum() > 50).ToList();
            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("Bob", records[0].Name);
        }

        private static dynamic CreateAggregateTestDb()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Name: "Alice", Age: 20);
            db.Test.Insert(Id: 2, Name: "Alice", Age: 30);
            db.Test.Insert(Id: 3, Name: "Bob", Age: 40);
            db.Test.Insert(Id: 4, Name: "Bob", Age: 50);
            return db;
        }

        [Test]
        public void ShouldWorkWithByteArrays()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Data: new byte[] { 0x1, 0x2, 0x3 });
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

        [Test]
        public void TestOrderBy()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            for (int i = 0; i < 10; i++)
            {
                db.Test.Insert(Id: i, Name: "Alice");
            }

            var records = db.Test.All().OrderByIdDescending().ToList();
            Assert.AreEqual(9, records[0].Id);
        }

        [Test]
        public void TestSkip()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            for (int i = 0; i < 10; i++)
            {
                db.Test.Insert(Id: i, Name: "Alice");
            }

            var records = db.Test.All().Skip(5).ToList();
            Assert.AreEqual(5, records.Count);
        }

        [Test]
        public void TestTake()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            for (int i = 0; i < 10; i++)
            {
                db.Test.Insert(Id: i, Name: "Alice");
            }

            var records = db.Test.All().Take(5).ToList();
            Assert.AreEqual(5, records.Count);
        }

        [Test]
        public void TestSkipAndTake()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            for (int i = 0; i < 10; i++)
            {
                db.Test.Insert(Id: i, Name: "Alice");
            }

            var records = db.Test.All().OrderByIdDescending().Skip(1).Take(1).ToList();
            Assert.AreEqual(1, records.Count);
            Assert.AreEqual(8, records[0].Id);
        }

        [Test]
        public void TestJoin()
        {
    var adapter = new InMemoryAdapter();
    adapter.ConfigureJoin("Customer", "ID", "Orders", "Order", "CustomerID", "Customer");
    Database.UseMockAdapter(adapter);
    var db = Database.Open();
    db.Customer.Insert(ID: 1, Name: "NASA");
    db.Customer.Insert(ID: 2, Name: "ACME");
    db.Order.Insert(ID: 1, CustomerID: 1, Date: new DateTime(1997, 1, 12));
    db.Order.Insert(ID: 2, CustomerID: 2, Date: new DateTime(2001, 1, 1));
    
    var customers = db.Customer.FindAll(db.Customer.Orders.Date < new DateTime(1999, 12, 31)).ToList();
    Assert.IsNotNull(customers);
    Assert.AreEqual(1, customers.Count);
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [Test]
        public void SeparateThreadsShouldSeeDifferentMocks()
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

        [Test]
        public void FindAllWhenUsingNamePropertyShouldWork()
        {
            var adapter = new InMemoryAdapter();
            adapter.ConfigureJoin("Users", "Id", "Categories", "Categories", "UserId", "User");
            Database.UseMockAdapter(adapter);
            var db = Database.Open();

            db.Users.Insert(Id: 1, Name: "Marcus");
            db.Users.Insert(Id: 2, Name: "Per");
            db.Categories.Insert(Id: 1, UserId: 1, Name: "Category 1");
            db.Categories.Insert(Id: 2, UserId: 2, Name: "Category 2");

            var categories = db.Users.FindAll(db.User.Categories.Name == "Category 1").ToList();
            Assert.NotNull(categories);
            Assert.AreEqual(1, categories.Count); 
        }

        [Test]
        public void FindAllWhenUsingAnyOldPropertyNameShouldWork()
        {
            var adapter = new InMemoryAdapter();
            adapter.ConfigureJoin("Users", "Id", "Categories", "Categories", "UserId", "User");
            Database.UseMockAdapter(adapter);
            var db = Database.Open();

            db.Users.Insert(Id: 1, UserName: "Marcus");
            db.Users.Insert(Id: 2, UserName: "Per");
            db.Categories.Insert(Id: 1, UserId: 1, CategoryName: "Category 1");
            db.Categories.Insert(Id: 2, UserId: 2, CategoryName: "Category 2");

            var categories = db.Users.FindAll(db.User.Categories.CategoryName == "Category 1").ToList();
            Assert.NotNull(categories);
            Assert.AreEqual(1, categories.Count); 
        }

        [Test]
        public void AutoIncrementShouldSet1ForAutoIncrementedColumnsWhenNoRowsInTable()
        {
            // Arrange
            var adapter = new InMemoryAdapter();
            adapter.SetAutoIncrementColumn("Users", "Id");

            Database.UseMockAdapter(adapter);
            var db = Database.Open();

            // Act
            var newId = db.Users.Insert(Name: "Marcus").Id;

            // Assert
            Assert.AreEqual(1, newId);
        }

        [Test]
        public void AutoIncrementShouldReturnNextIdInSequenceWhenOneRowExsists()
        {
            // Arrange
            var adapter = new InMemoryAdapter();
            adapter.SetAutoIncrementColumn("Users", "Id");

            Database.UseMockAdapter(adapter);
            var db = Database.Open();
            db.Users.Insert(Name: "Marcus");

            // Act
            var newId = db.Users.Insert(Name: "Per").Id;

            // Assert
            Assert.AreEqual(2, newId);
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
