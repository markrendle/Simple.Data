using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Simple.Data.UnitTest
{
    [TestFixture]
    public class BufferedEnumerableTest
    {
        [Test]
        public void ShouldCallCleanup()
        {
            // Arrange
            bool cleanedUp = false;

            var list = BufferedEnumerable.Create("ABC".ToIterator(), () => cleanedUp = true)
                    .ToList();

            // Act
            Assert.AreEqual(3, list.Count);

            SpinWait.SpinUntil(() => cleanedUp, 1000);

            Assert.True(cleanedUp);
        }
    }

    [TestFixture]
    public class MaybeTest
    {

        [Test]
        public void MaybeNoneShouldBeFalse()
        {
            Assert.False(Maybe<int>.None);
        }

        [Test]
        public void MaybeSomeShouldBeTrue()
        {
            Assert.True(Maybe<int>.Some(1));
        }

        [Test]
        public void IteratorShouldRun()
        {
            int n = 0;
            Func<Maybe<int>> iterator = () => ++n < 10 ? n : Maybe<int>.None;
            Maybe<int> maybe;
            while (maybe = iterator())
            {
                Assert.AreEqual(n, maybe.Value);
            }
            Assert.False(maybe);
        }
    }
}
