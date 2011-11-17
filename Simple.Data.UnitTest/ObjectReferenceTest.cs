namespace Simple.Data.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;

    [TestFixture]
    public class ObjectReferenceTest
    {
        static readonly dynamic Db = new Database(null);

        [Test]
        public void ObjectReferenceWithEqualsMakesExpression()
        {
            AssertHelper(Db.foo.id == 1, SimpleExpressionType.Equal, 1);
        }

        [Test]
        public void ObjectReferenceWithNotEqualsMakesExpression()
        {
            AssertHelper(Db.foo.id != 1, SimpleExpressionType.NotEqual, 1);
        }

        [Test]
        public void ObjectReferenceWithGreaterThanMakesExpression()
        {
            AssertHelper(Db.foo.id > 1, SimpleExpressionType.GreaterThan, 1);
        }

        [Test]
        public void ObjectReferenceWithLessThanMakesExpression()
        {
            AssertHelper(Db.foo.id < 1, SimpleExpressionType.LessThan, 1);
        }

        [Test]
        public void ObjectReferenceWithGreaterThanOrEqualMakesExpression()
        {
            AssertHelper(Db.foo.id >= 1, SimpleExpressionType.GreaterThanOrEqual, 1);
        }

        [Test]
        public void ObjectReferenceWithLessThanOrEqualMakesExpression()
        {
            AssertHelper(Db.foo.id <= 1, SimpleExpressionType.LessThanOrEqual, 1);
        }

        [Test]
        public void GetTopReturnsTopReference()
        {
            var foo = new ObjectReference("foo");
            var bar = new ObjectReference("bar", foo);
            var quux = new ObjectReference("quux", bar);

            Assert.AreEqual(foo, quux.GetTop());
        }

        private static void AssertHelper<T>(SimpleExpression actual, SimpleExpressionType expectedType, T expectedRightOperand)
        {
            Assert.AreEqual(Db.foo.id, actual.LeftOperand);
            Assert.AreEqual(expectedType, actual.Type);
            Assert.AreEqual(expectedRightOperand, actual.RightOperand);
        }
    }
}
