using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shitty.Data.Extensions;

namespace Shitty.Data.UnitTest
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
