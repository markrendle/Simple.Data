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

        [Test]
        public void SkipShouldSkip()
        {
            var runner = new DictionaryQueryRunner(SkipTakeSource(), new SkipClause(1));
            var actual = runner.Run().ToList();
            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(1, actual[0]["Row"]);
            Assert.AreEqual(2, actual[1]["Row"]);
        }

        [Test]
        public void TakeShouldTake()
        {
            var runner = new DictionaryQueryRunner(SkipTakeSource(), new TakeClause(2));
            var actual = runner.Run().ToList();
            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(0, actual[0]["Row"]);
            Assert.AreEqual(1, actual[1]["Row"]);
        }

        [Test]
        public void SkipAndTakeShouldSkipAndTake()
        {
            var runner = new DictionaryQueryRunner(SkipTakeSource(), new SkipClause(1), new TakeClause(1));
            var actual = runner.Run().ToList();
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(1, actual[0]["Row"]);
        }

        [Test]
        public void SkipAndTakeWithCountShouldSkipAndTakeAndGiveCount()
        {
            int count = 0;
            var runner = new DictionaryQueryRunner(SkipTakeSource(), new WithCountClause(n => count = n), new SkipClause(1), new TakeClause(1));
            var actual = runner.Run().ToList();
            Assert.AreEqual(3, count);
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual(1, actual[0]["Row"]);
        }

        #region Distinct sources

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

        #endregion

        #region Skip/Take/WithCount sources

        private static IEnumerable<IDictionary<string, object>> SkipTakeSource()
        {
            yield return new Dictionary<string, object>
                             {
                                 { "Row", 0 },
                             };
            yield return new Dictionary<string, object>
                             {
                                 { "Row", 1 },
                             };
            yield return new Dictionary<string, object>
                             {
                                 { "Row", 2 }
                             };
        }

        #endregion
    }
}
