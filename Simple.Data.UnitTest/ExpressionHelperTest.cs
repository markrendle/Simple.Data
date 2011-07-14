using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.UnitTest
{
    using NUnit.Framework;

    [TestFixture]
    public class ExpressionHelperTest
    {
        [Test]
        public void DictionaryToExpressionTest()
        {
            var dict = new Dictionary<string, object>
                           {
                               { "foo", 1 },
                               { "bar", 2 }
                           };

            var actual = ExpressionHelper.CriteriaDictionaryToExpression("quux", dict);

            Assert.AreEqual(SimpleExpressionType.And, actual.Type);

            var actualFirst = (SimpleExpression)actual.LeftOperand;
            var actualSecond = (SimpleExpression)actual.RightOperand;

            Assert.AreEqual("foo", ((ObjectReference)actualFirst.LeftOperand).GetName());
            Assert.AreEqual(SimpleExpressionType.Equal, actualFirst.Type);
            Assert.AreEqual(1, actualFirst.RightOperand);

            Assert.AreEqual("bar", ((ObjectReference)actualSecond.LeftOperand).GetName());
            Assert.AreEqual(SimpleExpressionType.Equal, actualSecond.Type);
            Assert.AreEqual(2, actualSecond.RightOperand);
        }
    }
}
