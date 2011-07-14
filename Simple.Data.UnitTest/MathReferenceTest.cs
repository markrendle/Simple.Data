namespace Simple.Data.UnitTest
{
    using NUnit.Framework;

    [TestFixture]
    public class MathReferenceTest
    {
        static readonly dynamic Db = new Database(null);

        [Test]
        public void MathReferenceWithEqualsMakesExpression()
        {
            AssertHelper(Db.foo.id + 1 == 1, SimpleExpressionType.Equal, 1);
        }

        [Test]
        public void MathReferenceWithNotEqualsMakesExpression()
        {
            AssertHelper(Db.foo.id - 1 != 1, SimpleExpressionType.NotEqual, 1);
        }

        [Test]
        public void MathReferenceWithGreaterThanMakesExpression()
        {
            AssertHelper(Db.foo.id * 1 > 1, SimpleExpressionType.GreaterThan, 1);
        }

        [Test]
        public void MathReferenceWithLessThanMakesExpression()
        {
            AssertHelper(Db.foo.id / 2 < 1, SimpleExpressionType.LessThan, 1);
        }

        [Test]
        public void MathReferenceWithGreaterThanOrEqualMakesExpression()
        {
            AssertHelper(Db.foo.id % 1 >= 1, SimpleExpressionType.GreaterThanOrEqual, 1);
        }

        [Test]
        public void MathReferenceWithLessThanOrEqualMakesExpression()
        {
            AssertHelper(Db.foo.id % 2 <= 1, SimpleExpressionType.LessThanOrEqual, 1);
        }

        private static void AssertHelper<T>(SimpleExpression actual, SimpleExpressionType expectedType, T expectedRightOperand)
        {
            Assert.AreEqual(Db.foo.id, ((MathReference)actual.LeftOperand).LeftOperand);
            Assert.AreEqual(expectedType, actual.Type);
            Assert.AreEqual(expectedRightOperand, actual.RightOperand);
        }
    }
}