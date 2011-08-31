using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.UnitTest
{
    using NUnit.Framework;

    [TestFixture]
    public class DictionaryQueryRunnerTest
    {
        [Test]
        public void DistinctShouldRemoveDuplicateRows()
        {
            var runner = new DictionaryQueryRunner(DuplicatingSource(), new DistinctClause());
            var actual = runner.Run().ToList();
            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(1, actual.Count(d => d.ContainsKey("Foo") && (string)d["Foo"] == "bar"));
            Assert.AreEqual(1, actual.Count(d => d.ContainsKey("Quux") && (string)d["Quux"] == "baz"));
        }

        [Test]
        public void ShouldNotRemoveDistinctRows()
        {
            var runner = new DictionaryQueryRunner(NonDuplicatingSource(), new DistinctClause());
            var actual = runner.Run().ToList();
            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(1, actual.Count(d => d.ContainsKey("Foo") && (string)d["Foo"] == "bar"));
            Assert.AreEqual(1, actual.Count(d => d.ContainsKey("Quux") && (string)d["Quux"] == "baz"));
        }

        private static IEnumerable<IDictionary<string, object>> DuplicatingSource()
        {
            yield return new Dictionary<string, object>
                             {
                                 { "Foo", "bar" },
                             };
            yield return new Dictionary<string, object>
                             {
                                 { "Quux", "baz" },
                             };
            yield return new Dictionary<string, object>
                             {
                                 { "Quux", "baz" },
                             };
        }

        private static IEnumerable<IDictionary<string, object>> NonDuplicatingSource()
        {
            yield return new Dictionary<string, object>
                             {
                                 { "Foo", "bar" },
                             };
            yield return new Dictionary<string, object>
                             {
                                 { "Quux", "baz" },
                             };
        }
    }
}
