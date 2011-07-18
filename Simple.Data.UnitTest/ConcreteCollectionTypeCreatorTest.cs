namespace Simple.Data.UnitTest
{
    using System.Collections;
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class ConcreteCollectionTypeCreatorTest
    {
        private IEnumerable<dynamic> Items()
        {
            yield return "Foo";
            yield return "Bar";
        }

        [Test]
        public void ListTest()
        {
            object result;
            ConcreteCollectionTypeCreator.TryCreate(typeof (List<string>), Items(), out result);
            Assert.IsNotNull(result);

            var list = result as List<string>;

            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.Contains("Foo"));
            Assert.IsTrue(list.Contains("Bar"));
        }

        [Test]
        public void SetTest()
        {
            object result;
            ConcreteCollectionTypeCreator.TryCreate(typeof(HashSet<string>), Items(), out result);
            Assert.IsNotNull(result);

            var @set = result as HashSet<string>;

            Assert.IsNotNull(@set);
            Assert.AreEqual(2, @set.Count);
            Assert.IsTrue(@set.Contains("Foo"));
            Assert.IsTrue(@set.Contains("Bar"));
        }

        [Test]
        public void ArrayListTest()
        {
            object result;
            ConcreteCollectionTypeCreator.TryCreate(typeof(ArrayList), Items(), out result);
            Assert.IsNotNull(result);

            var list = result as ArrayList;

            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.Contains("Foo"));
            Assert.IsTrue(list.Contains("Bar"));
        }
    }
}
