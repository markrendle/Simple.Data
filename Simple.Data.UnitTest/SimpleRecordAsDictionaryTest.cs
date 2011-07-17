using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.UnitTest
{
    using NUnit.Framework;

    [TestFixture]
    public class SimpleRecordAsDictionaryTest : DictionaryTestBase<string,object>
    {
        protected override IDictionary<string, object> CreateTarget()
        {
            return new SimpleRecord();
        }

        protected override KeyValuePair<string, object> CreateEntry(int index)
        {
            return new KeyValuePair<string, object>(index.ToString(), index);
        }
    }

    public abstract class DictionaryTestBase<TKey,TValue>
    {
        protected abstract IDictionary<TKey, TValue> CreateTarget();
        protected abstract KeyValuePair<TKey, TValue> CreateEntry(int index);

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
        public void AddKeyAndValue()
        {
            var target = CreateTarget();
            var entry = CreateEntry(0);

            target.Add(entry.Key, entry.Value);

            Assert.AreEqual(1, target.Count);
            Assert.IsTrue(target.ContainsKey(entry.Key));
            Assert.AreEqual(entry.Value, target[entry.Key]);
        }
    }
}
