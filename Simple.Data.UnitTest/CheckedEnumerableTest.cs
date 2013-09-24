namespace Simple.Data.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class CheckedEnumerableTest
    {
        [Test]
        public void DetectsEmpty()
        {
            var helper = new Helper<int>();
            var target = new CheckedEnumerable<int>(helper.GetEnumerable());
            Assert.IsTrue(target.IsEmpty);
            Assert.IsTrue(target.IsEmpty);
            Assert.DoesNotThrow(() => target.ToList());
            Assert.Throws<InvalidOperationException>(() => target.ToList());
            Assert.AreEqual(1, helper.UseCount);
        }

        [Test]
        public void DetectsOne()
        {
            var helper = new Helper<int>(1);
            var target = new CheckedEnumerable<int>(helper.GetEnumerable());
            Assert.IsFalse(target.IsEmpty);
            Assert.IsFalse(target.HasMoreThanOneValue);
            Assert.DoesNotThrow(() => target.ToList());
            Assert.Throws<InvalidOperationException>(() => target.ToList());
            Assert.AreEqual(1, helper.UseCount);
            Assert.AreEqual(1, target.Single);
        }

        [Test]
        public void DetectsMoreThanOne()
        {
            var helper = new Helper<int>(1,2,3,4);
            var target = new CheckedEnumerable<int>(helper.GetEnumerable());
            Assert.IsFalse(target.IsEmpty);
            Assert.IsTrue(target.HasMoreThanOneValue);
            Assert.DoesNotThrow(() => target.ToList());
            Assert.Throws<InvalidOperationException>(() => target.ToList());
            Assert.AreEqual(1, helper.UseCount);
        }

        class Helper<T>
        {
            private readonly T[] _items;

            public Helper(params T[] items)
            {
                UseCount = 0;
                _items = items;
            }

            public int UseCount { get; set; }

            public IEnumerable<T> GetEnumerable()
            {
                ++UseCount;
                for (int i = 0; i < _items.Length; i++)
                {
                    yield return _items[i];
                }
            }
        }
    }
}