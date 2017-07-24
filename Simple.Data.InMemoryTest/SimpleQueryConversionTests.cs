using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;

namespace Shitty.Data.InMemoryTest
{
    [TestFixture]
    public class SimpleQueryConversionTests
    {
        [Test]
        public void ShouldCastToList()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Name: "Alice");
            db.Test.Insert(Id: 2, Name: "Bob");
            List<Person> records = db.Test.All();
            Assert.IsNotNull(records);
            Assert.AreEqual(2, records.Count);
        }

        [Test]
        public void ShouldCastToPersonCollection()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Name: "Alice");
            db.Test.Insert(Id: 2, Name: "Bob");
            PersonCollection records = db.Test.All();
            Assert.IsNotNull(records);
            Assert.AreEqual(2, records.Count);
        }
        
        [Test]
        public void ShouldCastToIEnumerableOfPerson()
        {
            Database.UseMockAdapter(new InMemoryAdapter());
            var db = Database.Open();
            db.Test.Insert(Id: 1, Name: "Alice");
            db.Test.Insert(Id: 2, Name: "Bob");
            IEnumerable<Person> records = db.Test.All();
            Assert.IsNotNull(records);
            Assert.AreEqual(2, records.Count());
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PersonCollection : Collection<Person>
    {
        
    }
}