namespace Shitty.Data.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class SimpleRecordAsDictionaryTest
    {
        private IDictionary<string, object> CreateTarget()
        {
            return new SimpleRecord();
        }

        private KeyValuePair<string, object> CreateEntry(int index)
        {
            return new KeyValuePair<string, object>(index.ToString(), index);
        }

        [Test]
        public void AddKeyValuePair()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);

            target.Add(entry);

            Assert.AreEqual(1, target.Count);
            Assert.IsTrue(target.ContainsKey(entry.Key));
            Assert.AreEqual(entry.Value, target[entry.Key]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddDuplicateKeyValuePairShouldThrow()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);

            target.Add(entry);
            target.Add(entry);
        }

        [Test]
        public void AddKeyAndValue()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);

            target.Add(entry.Key, entry.Value);

            Assert.AreEqual(1, target.Count);
            Assert.IsTrue(target.ContainsKey(entry.Key));
            Assert.AreEqual(entry.Value, target[entry.Key]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddDuplicateKeyAndValueShouldThrow()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);

            target.Add(entry.Key, entry.Value);
            target.Add(entry.Key, entry.Value);
        }

        [Test]
        public void AfterClearCountShouldBeZero()
        {
            var target = CreateTarget();
            target.Add(CreateEntry(0));
            target.Add(CreateEntry(1));
            target.Clear();

            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void ContainsShouldReturnTrueForValidEntry()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            target.Add(entry);
            Assert.IsTrue(target.Contains(entry));
        }

        [Test]
        public void ContainsShouldReturnFalseForInvalidEntry()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            Assert.IsFalse(target.Contains(entry));
        }

        [Test]
        public void ContainsKeyShouldReturnTrueForValidKey()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            target.Add(entry);
            Assert.IsTrue(target.ContainsKey(entry.Key));
        }

        [Test]
        public void ContainsKeyShouldReturnFalseForInvalidKey()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            Assert.IsFalse(target.ContainsKey(entry.Key));
        }

        [Test]
        public void CopyToZeroBasedShouldCopyAllElements()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);
            var entry1 = CreateEntry(1);
            target.Add(entry0);
            target.Add(entry1);

            var array = new KeyValuePair<string,object>[2];
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

            var array = new KeyValuePair<string,object>[3];
            target.CopyTo(array, 1);

            Assert.AreEqual(entry0, array[1]);
            Assert.AreEqual(entry1, array[2]);
        }

        [Test]
        public void CountOnNewInstanceShouldBeZero()
        {
            var target = CreateTarget();
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void CountAfterAddShouldBeOne()
        {
            var target = CreateTarget();
            target.Add(CreateEntry(0));
            Assert.AreEqual(1, target.Count);
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
        public void KeysShouldContainKey()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            target.Add(entry);
            Assert.AreEqual(1, target.Keys.Count);
            Assert.AreEqual(entry.Key, target.Keys.Single());
        }

        [Test]
        public void KeysShouldContainKeys()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);
            var entry1 = CreateEntry(1);
            target.Add(entry0);
            target.Add(entry1);
            Assert.AreEqual(2, target.Keys.Count);
            Assert.AreEqual(entry0.Key, target.Keys.First());
            Assert.AreEqual(entry1.Key, target.Keys.Last());
        }

        [Test]
        public void RemoveKeyValuePairShouldRemoveIt()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            target.Add(entry);
            target.Remove(entry);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void RemoveKeyValuePairShouldOnlyRemoveIt()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);
            var entry1 = CreateEntry(1);
            target.Add(entry0);
            target.Add(entry1);
            target.Remove(entry0);
            Assert.AreEqual(1, target.Count);
            Assert.IsTrue(target.Contains(entry1));
        }

        [Test]
        public void RemoveKeyShouldRemoveIt()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            target.Add(entry);
            target.Remove(entry.Key);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void RemoveKeyShouldOnlyRemoveIt()
        {
            var target = CreateTarget();
            var entry0 = CreateEntry(0);
            var entry1 = CreateEntry(1);
            target.Add(entry0);
            target.Add(entry1);
            target.Remove(entry0.Key);
            Assert.AreEqual(1, target.Count);
            Assert.IsTrue(target.Contains(entry1));
        }

        [Test]
        public void TryGetValueShouldReturnTrueAndGetValue()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            target.Add(entry);

            object value;
            Assert.IsTrue(target.TryGetValue(entry.Key, out value));
            Assert.AreEqual(entry.Value, value);
        }

        [Test]
        public void TryGetValueShouldReturnFalseAndGetDefaultValue()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);

            object value;
            Assert.IsFalse(target.TryGetValue(entry.Key, out value));
            Assert.AreEqual(default(object), value);
        }

        [Test]
        public void IndexerShouldReturnCorrectValue()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);
            target.Add(entry);

            Assert.AreEqual(entry.Value, target[entry.Key]);
        }


        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void IndexerShouldThrowWithInvalidKey()
        {
            var target = CreateTarget();
            var x = target["INVALIDKEY"];
        }
    }
}
