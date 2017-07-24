namespace Shitty.Data.UnitTest
{
    using System;
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

        [Test]
        public void TryConvertElementShouldConvertStringToEnum()
        {
            var testCreator = new TestCreator();
            object result;
            Assert.IsTrue(testCreator.TestTryConvertElement(typeof(TestEnum), "Value", out result));
            Assert.AreEqual(TestEnum.Value, result);
        }

        [Test]
        public void TryConvertElementShouldConvertIntToEnum()
        {
            var testCreator = new TestCreator();
            object result;
            Assert.IsTrue(testCreator.TestTryConvertElement(typeof(TestEnum), 1, out result));
            Assert.AreEqual(TestEnum.Value, result);
        }

        [Test]
        public void TryConvertElementShouldConvertIntToNullableInt()
        {
            var testCreator = new TestCreator();
            object result;
            Assert.IsTrue(testCreator.TestTryConvertElement(typeof(int?), 1, out result));
            Assert.AreEqual(1, result);
        }

        enum TestEnum
        {
            None = 0,
            Value = 1
        }
    }

    class TestCreator : ConcreteCollectionTypeCreator.Creator
    {
        public override bool IsCollectionType(Type type)
        {
            return typeof (ICollection).IsAssignableFrom(type);
        }

        public override bool TryCreate(Type type, IEnumerable items, out object result)
        {
            throw new NotImplementedException();
        }

        public bool TestTryConvertElement(Type type, object value, out object result)
        {
            return TryConvertElement(type, value, out result);
        }
    }
}
