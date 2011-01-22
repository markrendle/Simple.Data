using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data;

namespace Simple.Data.UnitTest
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
            var range = "01/01/2011".to("31/01/2011");
            Assert.AreEqual(new DateTime(2011,1,1), range.Start);
            Assert.AreEqual(new DateTime(2011,1,31), range.End);
        }
    }
}
