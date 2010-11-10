using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.NExtLib.Tests
{
    [TestFixture]
    public class DateTimeExtensionsTests : AssertionHelper
    {
        [Test]
        public void ToIso8601String_formats_dates_correctly()
        {
            var date = new DateTime(2010, 2, 17, 13, 12, 34, 0, DateTimeKind.Utc);

            Assert.That(date.ToIso8601String(), Is.EqualTo("2010-02-17T13:12:34.0000000Z"));
        }
    }
}
