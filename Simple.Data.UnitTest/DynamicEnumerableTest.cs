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
            dynamic test = new DynamicEnumerable(new[] {"Hello", "World"});
            IEnumerable<string> strings = test.Cast<string>();
            Assert.AreEqual(2, strings.Count());
        }

        [Test]
        public void TestCastWithClass()
        {
            var dict = new Dictionary<string, object> {{"Name", "Bob"}};
            dynamic test = new DynamicEnumerable(new[] {new DynamicRecord(dict)});
            IEnumerable<Foo> foos = test.Cast<Foo>();
            Assert.AreEqual(1, foos.Count());
        }

        class Foo
        {
            public string Name { get; set; }
        }
    }
}
