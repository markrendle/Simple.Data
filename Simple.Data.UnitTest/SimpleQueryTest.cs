using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.UnitTest
{
    [TestFixture]
    public class SimpleQueryTest
    {
        [Test]
        public void WhereShouldSetCriteria()
        {
            var query = new SimpleQuery("foo");
            var criteria = new SimpleExpression(1, 1, SimpleExpressionType.Equal);
            query = query.Where(criteria);
            Assert.AreSame(criteria, query.Criteria);
        }

        [Test]
        public void SkipShouldSetSkipCount()
        {
            var query = new SimpleQuery("foo");
            query = query.Skip(42);
            Assert.AreEqual(42, query.SkipCount);
        }

        [Test]
        public void TakeShouldSetTakeCount()
        {
            var query = new SimpleQuery("foo");
            query = query.Take(42);
            Assert.AreEqual(42, query.TakeCount);
        }

        [Test]
        public void OrderByShouldSetOrderAscending()
        {
            var query = new SimpleQuery("foo");
            query = query.OrderBy(new DynamicReference("bar"));
            Assert.AreEqual("bar", query.Order.Single().Reference.GetName());
            Assert.AreEqual(OrderByDirection.Ascending, query.Order.Single().Direction);
        }

        [Test]
        public void ThenByShouldModifyOrderAscending()
        {
            var query = new SimpleQuery("foo");
            query = query.OrderBy(new DynamicReference("bar")).ThenBy(new DynamicReference("quux"));
            var actual = query.Order.ToArray();
            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("bar", actual[0].Reference.GetName());
            Assert.AreEqual(OrderByDirection.Ascending, actual[0].Direction);
            Assert.AreEqual("quux", actual[1].Reference.GetName());
            Assert.AreEqual(OrderByDirection.Ascending, actual[1].Direction);
        }

        [Test]
        public void OrderByDescendingShouldSetOrderDescending()
        {
            var query = new SimpleQuery("foo");
            query = query.OrderByDescending(new DynamicReference("bar"));
            Assert.AreEqual("bar", query.Order.Single().Reference.GetName());
            Assert.AreEqual(OrderByDirection.Descending, query.Order.Single().Direction);
        }

        [Test]
        public void ThenByDescendingShouldModifyOrderAscending()
        {
            var query = new SimpleQuery("foo");
            query = query.OrderBy(new DynamicReference("bar")).ThenByDescending(new DynamicReference("quux"));
            var actual = query.Order.ToArray();
            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("bar", actual[0].Reference.GetName());
            Assert.AreEqual(OrderByDirection.Ascending, actual[0].Direction);
            Assert.AreEqual("quux", actual[1].Reference.GetName());
            Assert.AreEqual(OrderByDirection.Descending, actual[1].Direction);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThenByWithoutOrderByShouldThrow()
        {
            new SimpleQuery("foo").ThenBy(new DynamicReference("bar"));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThenByDescendingWithoutOrderByShouldThrow()
        {
            new SimpleQuery("foo").ThenByDescending(new DynamicReference("bar"));
        }
    }
}
