using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.NExtLib.Unit;

namespace Simple.NExtLib.Tests
{
    [TestFixture]
    public class StringExtensionTests : AssertionHelper
    {
        [Test]
        public void EnsureStartsWith_should_prefix_string()
        {
            var actual = "bar".EnsureStartsWith("foo");

            actual.ShouldEqual("foobar");
        }

        [Test]
        public void EnsureStartsWith_should_not_prefix_string()
        {
            var actual = "foobar".EnsureStartsWith("foo");

            actual.ShouldEqual("foobar");
        }
    }
}
