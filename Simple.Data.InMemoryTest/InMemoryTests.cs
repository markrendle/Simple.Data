using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
