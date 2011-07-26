using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.UnitTest
{
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public class FutureTest
    {
        [Test]
        public void FutureShouldHaveHasValueFalseWhenCreated()
        {
            Action<int> setAction;
            var actual = Future<int>.Create(out setAction);
            Assert.IsFalse(actual.HasValue);
        }

        [Test]
        public void FutureShouldHaveValueAfterSetActionIsCalled()
        {
            Action<int> setAction;
            var actual = Future<int>.Create(out setAction);
            setAction(42);
            Assert.IsTrue(actual.HasValue);
            Assert.AreEqual(42, actual.Value);
        }

        [Test]
        public void FutureShouldImplicitlyCastToType()
        {
            Action<int> setAction;
            var actual = Future<int>.Create(out setAction);
            setAction(42);
            Assert.AreEqual(42, actual);
        }

        [Test]
        public void FutureShouldSpinWhenValueAccessedButNotSet()
        {
            Action<int> setAction;
            var actual = Future<int>.Create(out setAction);
            using (new Timer(_ => setAction(42), null, 100, Timeout.Infinite))
            {
                Assert.AreEqual(42, actual.Value);
            }
        }
    }
}
