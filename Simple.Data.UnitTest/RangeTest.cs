using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shitty.Data;

namespace Shitty.Data.UnitTest
{
    [TestFixture]
    public class RangeTest
    {
        [Test]
        public void IntRangeTest()
        {
            var range = 1.to(10);
            Assert.AreEqual(1, range.Start);
            Assert.AreEqual(10, range.End);
        }

        [Test]
        public void StringToDateRangeTest()
        {
            var range = "2011-01-01".to("2011-01-31");
            Assert.AreEqual(new DateTime(2011,1,1), range.Start);
            Assert.AreEqual(new DateTime(2011,1,31), range.End);
        }

        [Test]
        public void RangeToStringTest()
        {
            var range = 1.to(10);
            Assert.AreEqual("(1..10)", range.ToString());
        }

        [Test]
        public void RangeEqualityTest()
        {
            var range1 = 1.to(10);
            var range2 = 1.to(10);
            Assert.IsTrue(range1.Equals(range2));
        }

        [Test]
        public void RangeAsObjectEqualityTest()
        {
            object range1 = 1.to(10);
            object range2 = 1.to(10);
            Assert.IsTrue(range1.Equals(range2));
        }

        [Test]
        public void RangeEqualityOperatorTestTrue()
        {
            var range1 = 1.to(10);
            var range2 = 1.to(10);
            Assert.IsTrue(range1 == range2);
        }

        [Test]
        public void RangeEqualityOperatorTestFalse()
        {
            var range1 = 1.to(10);
            var range2 = 1.to(9);
            Assert.IsFalse(range1 == range2);
        }

        [Test]
        public void RangeInequalityOperatorTestFalse()
        {
            var range1 = 1.to(10);
            var range2 = 1.to(10);
            Assert.IsFalse(range1 != range2);
        }

        [Test]
        public void RangeInequalityOperatorTestTrue()
        {
            var range1 = 1.to(10);
            var range2 = 1.to(9);
            Assert.IsTrue(range1 != range2);
        }

        [Test]
        public void AsEnumerableFromIRange()
        {
            IRange range = 1.to(10);
            var enumerator = range.AsEnumerable().GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual(1, enumerator.Current);
            enumerator.MoveNext();
            Assert.AreEqual(10, enumerator.Current);
        }

        [Test]
        public void AsEnumerable()
        {
            var range = 1.to(10);
            var enumerator = range.AsEnumerable().GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual(1, enumerator.Current);
            enumerator.MoveNext();
            Assert.AreEqual(10, enumerator.Current);
        }

        [Test]
        public void AsEnumerableWithStep()
        {
            var range = 1.to(10);
            var enumerator = range.AsEnumerable(n => n + 1).GetEnumerator();
            for (int i = 1; i < 11; i++)
            {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(i, enumerator.Current);
            }
        }
    }
}
