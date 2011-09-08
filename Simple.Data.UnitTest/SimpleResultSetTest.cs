namespace Simple.Data.UnitTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    public class SimpleResultSetTest
    {
        private IEnumerable<dynamic> Records(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new SimpleRecord(new Dictionary<string, object> { { "Data", i.ToString() } });
            }
        }

        [Test]
        public void FirstReturnsFirstElement()
        {
            var list = new SimpleResultSet(Records(10));
            Assert.AreEqual("0", list.First().Data);
        }

        [Test]
        public void FirstThrowsOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.Throws<InvalidOperationException>(() => list.First());
        }

        [Test]
        public void FirstOrDefaultReturnsFirstElement()
        {
            var list = new SimpleResultSet(Records(10));
            Assert.AreEqual("0", list.FirstOrDefault().Data);
        }

        [Test]
        public void FirstOrDefaultReturnsNullOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.IsNull(list.FirstOrDefault());
        }

        [Test]
        public void FirstWithCriteriaReturnsMatch()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.AreEqual("8", list.First<TestType>(t => t.Data.EndsWith("8")).Data);
        }

        [Test]
        public void FirstWithFailingCriteriaThrows()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.Throws<InvalidOperationException>(() => list.First<TestType>(t => t.Data.EndsWith("A")));
        }

        [Test]
        public void FirstOrDefaultWithCriteriaReturnsMatch()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.AreEqual("8", list.FirstOrDefault<TestType>(t => t.Data.EndsWith("8")).Data);
        }

        [Test]
        public void FirstOrDefaultWithFailingCriteriaReturnsNull()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.IsNull(list.FirstOrDefault<TestType>(t => t.Data.EndsWith("A")));
        }

        [Test]
        public void LastReturnsLastElement()
        {
            var list = new SimpleResultSet(Records(10));
            Assert.AreEqual("9", list.Last().Data);
        }

        [Test]
        public void LastThrowsOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.Throws<InvalidOperationException>(() => list.Last());
        }

        [Test]
        public void LastOrDefaultReturnsLastElement()
        {
            var list = new SimpleResultSet(Records(10));
            Assert.AreEqual("9", list.LastOrDefault().Data);
        }

        [Test]
        public void LastOrDefaultReturnsNullOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.IsNull(list.LastOrDefault());
        }


        [Test]
        public void LastWithCriteriaReturnsMatch()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.AreEqual("18", list.Last<TestType>(t => t.Data.EndsWith("18")).Data);
        }

        [Test]
        public void LastWithFailingCriteriaThrows()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.Throws<InvalidOperationException>(() => list.Last<TestType>(t => t.Data.EndsWith("A")));
        }

        [Test]
        public void LastOrDefaultWithCriteriaReturnsMatch()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.AreEqual("18", list.LastOrDefault<TestType>(t => t.Data.EndsWith("18")).Data);
        }

        [Test]
        public void LastOrDefaultWithFailingCriteriaReturnsNull()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.IsNull(list.LastOrDefault<TestType>(t => t.Data.EndsWith("A")));
        }

        [Test]
        public void SingleReturnsSingleElement()
        {
            var list = new SimpleResultSet(Records(1));
            Assert.AreEqual("0", list.Single().Data);
        }

        [Test]
        public void SingleThrowsOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.Throws<InvalidOperationException>(() => list.Single());
        }

        [Test]
        public void SingleThrowsOnListWithMoreThanOneElement()
        {
            var list = new SimpleResultSet(Records(2));
            Assert.Throws<InvalidOperationException>(() => list.Single());
        }

        [Test]
        public void SingleOrDefaultReturnsSingleElement()
        {
            var list = new SimpleResultSet(Records(1));
            Assert.AreEqual("0", list.SingleOrDefault().Data);
        }

        [Test]
        public void SingleOrDefaultReturnsNullOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.IsNull(list.SingleOrDefault());
        }

        [Test]
        public void SingleOrDefaultThrowsOnListWithMoreThanOneElement()
        {
            var list = new SimpleResultSet(Records(2));
            Assert.Throws<InvalidOperationException>(() => list.SingleOrDefault());
        }
        
        [Test]
        public void SingleWithCriteriaMatchingOneRecordReturnsMatch()
        {
            var list = new SimpleResultSet(Records(10));
            Assert.AreEqual("8", list.Single<TestType>(t => t.Data.EndsWith("8")).Data);
        }

        [Test]
        public void SingleWithCriteriaMatchingMultipleRecordsThrows()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.Throws<InvalidOperationException>(() => list.Single<TestType>(t => t.Data.EndsWith("8")));
        }

        [Test]
        public void SingleWithFailingCriteriaThrows()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.Throws<InvalidOperationException>(() => list.Single<TestType>(t => t.Data.EndsWith("A")));
        }

        [Test]
        public void SingleOrDefaultWithCriteriaReturnsMatch()
        {
            var list = new SimpleResultSet(Records(10));
            Assert.AreEqual("8", list.SingleOrDefault<TestType>(t => t.Data.EndsWith("8")).Data);
        }

        [Test]
        public void SingleOrDefaultWithFailingCriteriaReturnsNull()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.IsNull(list.SingleOrDefault<TestType>(t => t.Data.EndsWith("A")));
        }

        [Test]
        public void SingleOrDefaultWithCriteriaMatchingMultipleRecordsThrows()
        {
            var list = new SimpleResultSet(Records(20));
            Assert.Throws<InvalidOperationException>(() => list.SingleOrDefault<TestType>(t => t.Data.EndsWith("8")));
        }

        [Test]
        public void GenericFirstReturnsFirstElement()
        {
            var list = new SimpleResultSet(Records(10));
            Assert.AreEqual("0", list.First<TestType>().Data);
        }

        [Test]
        public void GenericFirstThrowsOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.Throws<InvalidOperationException>(() => list.First<TestType>());
        }

        [Test]
        public void GenericFirstOrDefaultReturnsFirstElement()
        {
            var list = new SimpleResultSet(Records(10));
            Assert.AreEqual("0", list.FirstOrDefault<TestType>().Data);
        }

        [Test]
        public void GenericFirstOrDefaultReturnsNullOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.IsNull(list.FirstOrDefault<TestType>());
        }

        [Test]
        public void GenericLastReturnsLastElement()
        {
            var list = new SimpleResultSet(Records(10));
            Assert.AreEqual("9", list.Last<TestType>().Data);
        }

        [Test]
        public void GenericLastThrowsOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.Throws<InvalidOperationException>(() => list.Last<TestType>());
        }

        [Test]
        public void GenericLastOrDefaultReturnsLastElement()
        {
            var list = new SimpleResultSet(Records(10));
            Assert.AreEqual("9", list.LastOrDefault<TestType>().Data);
        }

        [Test]
        public void GenericLastOrDefaultReturnsNullOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.IsNull(list.LastOrDefault<TestType>());
        }

        [Test]
        public void GenericSingleReturnsSingleElement()
        {
            var list = new SimpleResultSet(Records(1));
            Assert.AreEqual("0", list.Single<TestType>().Data);
        }

        [Test]
        public void GenericSingleThrowsOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.Throws<InvalidOperationException>(() => list.Single<TestType>());
        }

        [Test]
        public void GenericSingleThrowsOnListWithMoreThanOneElement()
        {
            var list = new SimpleResultSet(Records(2));
            Assert.Throws<InvalidOperationException>(() => list.Single<TestType>());
        }

        [Test]
        public void GenericSingleOrDefaultReturnsSingleElement()
        {
            var list = new SimpleResultSet(Records(1));
            Assert.AreEqual("0", list.SingleOrDefault<TestType>().Data);
        }

        [Test]
        public void GenericSingleOrDefaultReturnsNullOnEmptyList()
        {
            var list = new SimpleResultSet(Records(0));
            Assert.IsNull(list.SingleOrDefault<TestType>());
        }

        [Test]
        public void GenericSingleOrDefaultThrowsOnListWithMoreThanOneElement()
        {
            var list = new SimpleResultSet(Records(2));
            Assert.Throws<InvalidOperationException>(() => list.SingleOrDefault<TestType>());
        }

        [Test]
        public void ConvertsToGenericList()
        {
            dynamic list = new SimpleResultSet(Records(1));
            List<dynamic> generic = list;
            Assert.IsNotNull(generic);
            Assert.AreEqual("0", generic[0].Data);
        }

        [Test]
        public void ConvertsToNonGenericList()
        {
            dynamic list = new SimpleResultSet(Records(1));
            ArrayList generic = list;
            Assert.IsNotNull(generic);
            Assert.AreEqual("0", ((dynamic)generic[0]).Data);
        }

        [Test]
        public void ConvertsToHashSet()
        {
            dynamic list = new SimpleResultSet(Records(1));
            HashSet<dynamic> generic = list;
            Assert.IsNotNull(generic);
            Assert.AreEqual("0", generic.Single().Data);
        } 

        [Test]
        public void ToArrayCreatesArray()
        {
            dynamic list = new SimpleResultSet(Records(1));
            dynamic[] array = list.ToArray();

            Assert.IsNotNull(array);
            Assert.AreEqual(1, array.Length);
            Assert.AreEqual("0", array[0].Data);
        }

        [Test]
        public void ToGenericArrayCreatesTypedArray()
        {
            dynamic list = new SimpleResultSet(Records(1));
            TestType[] array = list.ToArray<TestType>();

            Assert.IsNotNull(array);
            Assert.AreEqual(1, array.Length);
            Assert.AreEqual("0", array[0].Data);
        }

        [Test]
        public void ToListCreatesList()
        {
            dynamic list = new SimpleResultSet(Records(1));
            List<dynamic> array = list.ToList();

            Assert.IsNotNull(array);
            Assert.AreEqual(1, array.Count);
            Assert.AreEqual("0", array[0].Data);
        }

        [Test]
        public void ToGenericListCreatesTypedList()
        {
            dynamic list = new SimpleResultSet(Records(1));
            List<TestType> converted = list.ToList<TestType>();

            Assert.IsNotNull(converted);
            Assert.AreEqual(1, converted.Count);
            Assert.AreEqual("0", converted[0].Data);
        }

        [Test]
        public void CastToGenericCreatesTypedList()
        {
            dynamic list = new SimpleResultSet(Records(1));
            IEnumerable<TestType> converted = list.Cast<TestType>();

            Assert.IsNotNull(converted);
            Assert.AreEqual(1, converted.Count());
            Assert.AreEqual("0", converted.First().Data);
        }
    }

    class TestType
    {
        public string Data { get; set; }
    }
}