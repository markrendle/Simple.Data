namespace Simple.Data.UnitTest
{
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class DictionaryClonerTest
    {
        [Test]
        public void ClonedDictionaryShouldBeOfSameType()
        {
            var target = new DictionaryCloner();

            var original = new SortedDictionary<string, object>();
            var clone = target.CloneDictionary(original);
            Assert.AreNotSame(original, clone);
            Assert.IsInstanceOf<SortedDictionary<string,object>>(clone);
        }

        [Test]
        public void NestedDictionaryShouldBeCloned()
        {
            var target = new DictionaryCloner();
            var nested = new Dictionary<string, object> { { "Answer", 42 } };
            var original = new Dictionary<string, object> { { "Nested", nested } };

            var clone = target.CloneDictionary(original);

            Assert.IsTrue(clone.ContainsKey("Nested"));
            var nestedClone = clone["Nested"] as Dictionary<string, object>;
            Assert.IsNotNull(nestedClone);
            Assert.AreNotSame(nested, nestedClone);
            Assert.IsTrue(nestedClone.ContainsKey("Answer"));
            Assert.AreEqual(42, nestedClone["Answer"]);
        }

        [Test]
        public void NestedDictionariesShouldBeCloned()
        {
            var target = new DictionaryCloner();
            var nested = new List<Dictionary<string, object>> { new Dictionary<string, object> { { "Answer", 42 } } };
            var original = new Dictionary<string, object> { { "Nested", nested } };

            var clone = target.CloneDictionary(original);

            Assert.IsTrue(clone.ContainsKey("Nested"));
            var nestedClone = clone["Nested"] as List<Dictionary<string, object>>;
            Assert.IsNotNull(nestedClone);
            Assert.AreNotSame(nested, nestedClone);
            Assert.IsTrue(nestedClone.Count == 1);
            Assert.AreEqual(42, nestedClone[0]["Answer"]);
        }
    }
}