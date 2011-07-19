namespace Simple.Data.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;

    [TestFixture]
    public class SimpleListAsListTest
    {
        private IList<object> CreateTarget()
        {
            return new SimpleList(Enumerable.Empty<string>());
        }

        private object CreateEntry(int seed)
        {
            return seed.ToString();
        }

        [Test]
        public void AddShouldAddItem()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            target.Add(entry);
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(entry, target[0]);
        }

        [Test]
        public void ClearShouldRemoveAllEntries()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);

            target.Add(entry);

            target.Clear();
            Assert.AreEqual(0, target.Count);
            Assert.IsFalse(target.Contains(entry));
        }

        [Test]
        public void WhenListContainsItemContainsShouldBeTrue()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);

            target.Add(entry);

            Assert.IsTrue(target.Contains(entry));
        }

        [Test]
        public void WhenListDoesNotContainItemContainsShouldBeFalse()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);

            Assert.IsFalse(target.Contains(entry));
        }

        [Test]
        public void NewListShouldHaveCountEqualToZero()
        {
            var target = CreateTarget();

            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void ListWithOneItemShouldHaveCountEqualToOne()
        {
            var target = CreateTarget();
            target.Add(CreateEntry(0));

            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void CopyToZeroBasedShouldCopyAllElements()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);
            var entry1 = CreateEntry(1);
            target.Add(entry0);
            target.Add(entry1);

            var array = new object[2];
            target.CopyTo(array, 0);

            Assert.AreEqual(entry0, array[0]);
            Assert.AreEqual(entry1, array[1]);
        }

        [Test]
        public void CopyToNonZeroBasedShouldCopyElementsFromIndex()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);
            var entry1 = CreateEntry(1);
            target.Add(entry0);
            target.Add(entry1);

            var array = new object[3];
            target.CopyTo(array, 1);

            Assert.AreEqual(entry0, array[1]);
            Assert.AreEqual(entry1, array[2]);
        }

        [Test]
        public void EnumeratorTest()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            target.Add(entry);

            var enumerator = target.GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(entry, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void NonGenericEnumeratorTest()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            target.Add(entry);

            var enumerator = ((IEnumerable)target).GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(entry, enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [Test]
        public void IndexOfShouldReturnCorrectValues()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);
            var entry1 = CreateEntry(1);

            target.Add(entry0);
            target.Add(entry1);

            Assert.AreEqual(0, target.IndexOf(entry0));
            Assert.AreEqual(1, target.IndexOf(entry1));
        }

        [Test]
        public void InsertShouldPutItemAtCorrectIndex()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);
            var entry1 = CreateEntry(1);

            target.Add(entry1);
            Assert.AreEqual(0, target.IndexOf(entry1));

            target.Insert(0, entry0);

            Assert.AreEqual(0, target.IndexOf(entry0));
            Assert.AreEqual(1, target.IndexOf(entry1));
        }

        [Test]
        public void RemoveShouldRemoveEntry()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);

            target.Add(entry0);
            target.Remove(entry0);
            Assert.IsFalse(target.Contains(entry0));
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void RemoveAtShouldRemoveEntryAtCorrectIndex()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);
            var entry1 = CreateEntry(1);

            target.Add(entry0);
            target.Add(entry1);

            target.RemoveAt(0);
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(target[0], entry1);
        }

        [Test]
        public void GetIndexerTest()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            target.Add(entry);

            Assert.AreEqual(entry, target[0]);
        }

        [Test]
        public void SetIndexerTest()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);
            var entry1 = CreateEntry(1);
            target.Add(entry0);
            target[0] = entry1;

            Assert.AreEqual(entry1, target[0]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetIndexerBeyondSizeOfListShouldThrowException()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);

            target[0] = entry0;
        }
    }
}
