using System.Collections.Generic;
using NExtLib.Unit;
using NUnit.Framework;
using Simple.NExtLib.Linq;

namespace Simple.NExtLib.Tests.Linq
{
    [TestFixture]
    public class EnumerableExtensionTests
    {
        private readonly IEnumerable<string> TestList = new List<string> { "Foo", "Bar", "Quux" };

        [Test]
        public void TestWithIndex()
        {
            int expectedIndex = 0;

            foreach (var item in TestList.WithIndex())
            {
                item.Item2.ShouldEqual(expectedIndex);
                expectedIndex++;
            }
        }
    }
}
