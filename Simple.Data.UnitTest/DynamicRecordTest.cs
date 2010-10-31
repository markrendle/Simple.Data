using Simple.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Simple.Data.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for DynamicRecordTest and is intended
    ///to contain all DynamicRecordTest Unit Tests
    ///</summary>
    [TestFixture()]
    public class DynamicRecordTest
    {
        /// <summary>
        ///A test for DynamicRecord Constructor
        ///</summary>
        [Test()]
        public void DynamicRecordDictionaryConstructorTest()
        {
            IDictionary<string, object> data = new Dictionary<string, object>
                                                   {
                                                       { "Name", "Bob" },
                                                       { "Age", 42 }
                                                   };
            dynamic target = new DynamicRecord(data);
            Assert.AreEqual("Bob", target.Name);
            Assert.AreEqual(42, target.Age);
        }

        /// <summary>
        ///A test for DynamicRecord Constructor
        ///</summary>
        [Test()]
        public void DynamicRecordSetterTest()
        {
            dynamic target = new DynamicRecord();
            target.Name = "Bob";
            Assert.AreEqual("Bob", target.Name);
        }

        [Test]
        public void DynamicCastTest()
        {
            dynamic target = new DynamicRecord();
            target.Name = "Bob";
            target.Age = 42;

            User user = target;
            Assert.AreEqual("Bob", user.Name);
            Assert.AreEqual(42, user.Age);
        }

        internal class User
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
