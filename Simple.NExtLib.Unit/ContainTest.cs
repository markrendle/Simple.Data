using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NExtLib.TestExtensions
{
    public class ContainTest : IEnumerableTest
    {
        public void RunTest<T>(T expected, IEnumerable<T> actual)
        {
            Assert.IsTrue(actual.Contains(expected));
        }
    }
}
