using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.UnitTest
{
    using NUnit.Framework;

    [TestFixture]
    class SpecialReferenceTests
    {
        [Test]
        public void TestExistsReference()
        {
            var actual = new ExistsSpecialReference();
            Assert.AreEqual("EXISTS", actual.Name);
        }

        [Test]
        public void TestCountReference()
        {
            var actual = new CountSpecialReference();
            Assert.AreEqual("COUNT", actual.Name);
        }

        [Test]
        public void TestSimpleEmptyExpression()
        {
            var actual = new SimpleEmptyExpression();
            Assert.IsNull(actual.LeftOperand);
            Assert.AreEqual(SimpleExpressionType.Empty, actual.Type);
            Assert.IsNull(actual.RightOperand);
        }
    }
}
