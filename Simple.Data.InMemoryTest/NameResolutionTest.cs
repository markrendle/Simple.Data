using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.InMemoryTest
{
    using NUnit.Framework;

    [TestFixture]
    class NameResolutionTest
    {
        [Test]
        public void InsertAndFindByTableNameResolvesCorrectlyWithHomogenisedStringComparer()
        {
            var inMemoryAdapter = new InMemoryAdapter(new AdoCompatibleComparer());

            Database.UseMockAdapter(inMemoryAdapter);
            var db = Database.Open();

            db.CUSTOMER.Insert(ID: 1, NAME: "ACME");

            var actual = db.Customers.FindById(1);
            Assert.IsNotNull(actual);
            Assert.AreEqual("ACME", actual.Name);
        }
        
        [Test]
        public void UpdateTableNameResolvesCorrectlyWithHomogenisedStringComparer()
        {
            var inMemoryAdapter = new InMemoryAdapter(new AdoCompatibleComparer());

            Database.UseMockAdapter(inMemoryAdapter);
            var db = Database.Open();

            db.CUSTOMER.Insert(ID: 1, NAME: "ACME");

            db.Customers.UpdateById(Id: 1, Name: "ACME Inc.");
            var actual = db.Customers.FindById(1);
            Assert.IsNotNull(actual);
            Assert.AreEqual("ACME Inc.", actual.Name);
        }        

        [Test]
        public void DeleteTableNameResolvesCorrectlyWithHomogenisedStringComparer()
        {
            var inMemoryAdapter = new InMemoryAdapter(new AdoCompatibleComparer());

            Database.UseMockAdapter(inMemoryAdapter);
            var db = Database.Open();

            db.CUSTOMER.Insert(ID: 1, NAME: "ACME");

            var actual = db.Customers.FindById(1);
            Assert.IsNotNull(actual);

            db.Customers.DeleteById(1);
            actual = db.Customers.FindById(1);
            Assert.IsNull(actual);
        }
    }
}
