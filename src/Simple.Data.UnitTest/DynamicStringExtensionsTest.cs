using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data.Extensions;

namespace Simple.Data.UnitTest
{
    [TestFixture]
    public class DynamicStringExtensionsTest
    {
        [Test]
        public void PascalToSnakeTest()
        {
            var actual = "NameAndPostcode".ToSnakeCase();
            Assert.AreEqual("name_and_postcode", actual, "Snake case incorrect.");
        }
    }
}
