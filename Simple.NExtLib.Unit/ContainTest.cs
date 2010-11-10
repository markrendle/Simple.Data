using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Simple.NExtLib.Unit
{
    public class ContainTest : IEnumerableTest
    {
        public void RunTest<T>(T expected, IEnumerable<T> actual)
        {
            Assert.IsTrue(actual.Contains(expected));
        }
    }
}
