namespace Simple.Data.UnitTest
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class MaybeTest
    {

        [Test]
        public void MaybeNoneShouldBeFalse()
        {
            Assert.False(Maybe<int>.None.HasValue);
        }

        [Test]
        public void MaybeSomeShouldBeTrue()
        {
            Assert.True(Maybe<int>.Some(1).HasValue);
        }

        [Test]
        public void IteratorShouldRun()
        {
            int n = 0;
            Func<Maybe<int>> iterator = () => ++n < 10 ? n : Maybe<int>.None;
            Maybe<int> maybe;
            while ((maybe = iterator()).HasValue)
            {
                Assert.AreEqual(n, maybe.Value);
            }
            Assert.False(maybe.HasValue);
        }

        [Test]
        public void NoneOfSameTypeShouldBeEqual()
        {
            Assert.AreEqual(Maybe<int>.None, Maybe<int>.None);
        }

        [Test]
        public void NoneOfDifferentTypeShouldNotBeEqual()
        {
            Assert.AreNotEqual(Maybe<int>.None, Maybe<long>.None);
        }

        [Test]
        public void NoneValueIsDefault()
        {
            Assert.AreEqual(default(int), Maybe<int>.None.Value);
            Assert.IsNull(Maybe<object>.None.Value);
        }

        [Test]
        public void NoneToStringIsEmptyString()
        {
            Assert.AreEqual(string.Empty, Maybe<int>.None.ToString());
        }

        [Test]
        public void SomeEqualityTrueTest()
        {
            var first = Maybe.Some(42);
            var second = Maybe.Some(42);
            Assert.IsTrue(first == second);
        }

        [Test]
        public void SomeEqualityFalseTest()
        {
            var first = Maybe.Some(42);
            var second = Maybe.Some(43);
            Assert.IsFalse(first == second);
        }

        [Test]
        public void SomeInequalityFalseTest()
        {
            var first = Maybe.Some(42);
            var second = Maybe.Some(42);
            Assert.IsFalse(first != second);
        }

        [Test]
        public void SomeInequalityTrueTest()
        {
            var first = Maybe.Some(42);
            var second = Maybe.Some(43);
            Assert.IsTrue(first != second);
        }
    }
}