namespace Simple.Data.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class SimpleListTest
    {
        private IEnumerable<object> Strings(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return i.ToString();
            }
        }
        
        [Test]
        public void ElementAtOrDefaultReturnsCorrectElement()
        {
            var list = new SimpleList(Strings(2));
            Assert.AreEqual("0", list.ElementAtOrDefault(0));
            Assert.AreEqual("1", list.ElementAtOrDefault(1));
        }

        [Test]
        public void ElementAtOrDefaultReturnsNullForOutOfRange()
        {
            var list = new SimpleList(Strings(1));
            Assert.IsNull(list.ElementAtOrDefault(2));
        }

        [Test]
        public void ElementAtReturnsCorrectElement()
        {
            var list = new SimpleList(Strings(2));
            Assert.AreEqual("0", list.ElementAt(0));
            Assert.AreEqual("1", list.ElementAt(1));
        }

        [Test]
        public void ElementAtThrowsForOutOfRange()
        {
            var list = new SimpleList(Strings(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => list.ElementAt(2));
        }

        [Test]
        public void FirstReturnsFirstElement()
        {
            var list = new SimpleList(Strings(10));
            Assert.AreEqual("0", list.First());
        }

        [Test]
        public void FirstThrowsOnEmptyList()
        {
            var list = new SimpleList(Strings(0));
            Assert.Throws<InvalidOperationException>(() => list.First());
        }

        [Test]
        public void FirstOrDefaultReturnsFirstElement()
        {
            var list = new SimpleList(Strings(10));
            Assert.AreEqual("0", list.FirstOrDefault());
        }

        [Test]
        public void FirstOrDefaultReturnsNullOnEmptyList()
        {
            var list = new SimpleList(Strings(0));
            Assert.IsNull(list.FirstOrDefault());
        }

        [Test]
        public void LastReturnsLastElement()
        {
            var list = new SimpleList(Strings(10));
            Assert.AreEqual("9", list.Last());
        }

        [Test]
        public void LastThrowsOnEmptyList()
        {
            var list = new SimpleList(Strings(0));
            Assert.Throws<InvalidOperationException>(() => list.Last());
        }

        [Test]
        public void LastOrDefaultReturnsLastElement()
        {
            var list = new SimpleList(Strings(10));
            Assert.AreEqual("9", list.LastOrDefault());
        }

        [Test]
        public void LastOrDefaultReturnsNullOnEmptyList()
        {
            var list = new SimpleList(Strings(0));
            Assert.IsNull(list.LastOrDefault());
        }

        [Test]
        public void SingleReturnsSingleElement()
        {
            var list = new SimpleList(Strings(1));
            Assert.AreEqual("0", list.Single());
        }

        [Test]
        public void SingleThrowsOnEmptyList()
        {
            var list = new SimpleList(Strings(0));
            Assert.Throws<InvalidOperationException>(() => list.Single());
        }

        [Test]
        public void SingleThrowsOnListWithMoreThanOneElement()
        {
            var list = new SimpleList(Strings(2));
            Assert.Throws<InvalidOperationException>(() => list.Single());
        }

        [Test]
        public void SingleOrDefaultReturnsSingleElement()
        {
            var list = new SimpleList(Strings(1));
            Assert.AreEqual("0", list.SingleOrDefault());
        }

        [Test]
        public void SingleOrDefaultReturnsNullOnEmptyList()
        {
            var list = new SimpleList(Strings(0));
            Assert.IsNull(list.SingleOrDefault());
        }

        [Test]
        public void SingleOrDefaultThrowsOnListWithMoreThanOneElement()
        {
            var list = new SimpleList(Strings(2));
            Assert.Throws<InvalidOperationException>(() => list.SingleOrDefault());
        }

        [Test]
        public void SimpleListIsNotReadOnly()
        {
            var list = new SimpleList(Strings(0));
            Assert.IsFalse(list.IsReadOnly);
        }

        [Test]
        public void ConvertsToGenericList()
        {
            dynamic list = new SimpleList(Strings(1));
            List<string> generic = list;
            Assert.IsNotNull(generic);
            Assert.AreEqual("0", generic[0]);
        }

        [Test]
        public void ConvertsToNonGenericList()
        {
            dynamic list = new SimpleList(Strings(1));
            ArrayList generic = list;
            Assert.IsNotNull(generic);
            Assert.AreEqual("0", generic[0]);
        }

        [Test]
        public void ConvertsToHashSet()
        {
            dynamic list = new SimpleList(Strings(1));
            HashSet<string> generic = list;
            Assert.IsNotNull(generic);
            Assert.AreEqual("0", generic.Single());
        }
    }
}