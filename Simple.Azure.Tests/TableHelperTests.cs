using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Simple.Azure.Helpers;

namespace Simple.Azure.Tests
{
    [TestFixture]
    public class TableHelperTests : AssertionHelper
    {
        [Test]
        public void ReadTableList_does_not_throw_exception()
        {
            using (var stream = CreateDummyResponseStream())
            {
                Assert.DoesNotThrow(new TestDelegate(() => TableHelper.ReadTableList(stream)));
            }
        }

        [Test]
        public void ReadTableList_gets_mytable()
        {
            string[] actual;

            using (var stream = CreateDummyResponseStream())
            {
                actual = TableHelper.ReadTableList(stream).ToArray();
            }

            Assert.That(actual, Contains("mytable"));
        }

        private static Stream CreateDummyResponseStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.QueryTablesResponseText));
        }
    }
}
