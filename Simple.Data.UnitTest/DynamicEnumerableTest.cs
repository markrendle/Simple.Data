using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.UnitTest
{
    [TestFixture]
    public class DynamicEnumerableTest
    {
        [Test]
        public void TestCast()
        {
            dynamic test = new SimpleResultSet(new[] {"Hello", "World"});
            IEnumerable<string> strings = test.Cast<string>();
            Assert.AreEqual(2, strings.Count());
        }

        [Test]
        public void TestOfType()
        {
            dynamic test = new SimpleResultSet(new dynamic[] { "Hello", 1 });
            IEnumerable<int> ints = test.OfType<int>();
            Assert.AreEqual(1, ints.Count());
            Assert.AreEqual(1, ints.Single());
        }

        [Test]
        public void TestCastWithClass()
        {
            var dict = new Dictionary<string, object>(HomogenizedEqualityComparer.DefaultInstance) {{"Name", "Bob"}};
            dynamic test = new SimpleResultSet(new[] {new SimpleRecord(dict)});
            IEnumerable<Foo> foos = test.Cast<Foo>();
            Assert.AreEqual(1, foos.Count());
        }

        [Test]
        public void TestCastWithForeach()
        {
            var dict = new Dictionary<string, object>(HomogenizedEqualityComparer.DefaultInstance) { { "Name", "Bob" } };
            dynamic test = new SimpleResultSet(new[] { new SimpleRecord(dict) });
            foreach (Foo foo in test)
            {
                Assert.AreEqual("Bob", foo.Name);
            }
        }

        class Foo
        {
            public string Name { get; set; }
        }
    }
}
