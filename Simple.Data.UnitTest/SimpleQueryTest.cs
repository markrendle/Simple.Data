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
            var query = new SimpleQuery(null, "foo");
            var criteria = new SimpleExpression(1, 1, SimpleExpressionType.Equal);
            query = query.Where(criteria);
            Assert.AreSame(criteria, query.Clauses.OfType<WhereClause>().Single().Criteria);
        }

        [Test]
        public void SkipShouldSetSkipCount()
        {
            var query = new SimpleQuery(null, "foo");
            query = query.Skip(42);
            Assert.IsNotNull(query.Clauses.OfType<SkipClause>().FirstOrDefault());
            Assert.AreEqual(42, query.Clauses.OfType<SkipClause>().First().Count);
        }

        [Test]
        public void TakeShouldSetTakeCount()
        {
            var query = new SimpleQuery(null, "foo");
            query = query.Take(42);
            Assert.IsNotNull(query.Clauses.OfType<TakeClause>().FirstOrDefault());
            Assert.AreEqual(42, query.Clauses.OfType<TakeClause>().First().Count);
        }

        [Test]
        public void OrderByShouldSetOrderAscending()
        {
            var query = new SimpleQuery(null, "foo");
            query = query.OrderBy(new ObjectReference("bar"));
            Assert.AreEqual("bar", query.Clauses.OfType<OrderByClause>().Single().Reference.GetName());
            Assert.AreEqual(OrderByDirection.Ascending, query.Clauses.OfType<OrderByClause>().Single().Direction);
        }

        [Test]
        public void OrderByWithDescendingDirectionShouldSetOrderDescending()
        {
            var query = new SimpleQuery(null, "foo");
            query = query.OrderBy(new ObjectReference("bar"), OrderByDirection.Descending);
            Assert.AreEqual("bar", query.Clauses.OfType<OrderByClause>().Single().Reference.GetName());
            Assert.AreEqual(OrderByDirection.Descending, query.Clauses.OfType<OrderByClause>().Single().Direction);
        }

        [Test]
        public void OrderByBarShouldSetOrderAscending()
        {
            dynamic query = new SimpleQuery(null, "foo");
            SimpleQuery actual = query.OrderByBar();
            Assert.AreEqual("bar", actual.Clauses.OfType<OrderByClause>().Single().Reference.GetName().ToLowerInvariant());
            Assert.AreEqual(OrderByDirection.Ascending, actual.Clauses.OfType<OrderByClause>().Single().Direction);
        }

        [Test]
        public void OrderByBarThenByQuuxShouldSetOrderAscending()
        {
            dynamic query = new SimpleQuery(null, "foo");
            SimpleQuery actual = query.OrderByBar().ThenByQuux();
            Assert.AreEqual("bar", actual.Clauses.OfType<OrderByClause>().First().Reference.GetName().ToLowerInvariant());
            Assert.AreEqual("quux", actual.Clauses.OfType<OrderByClause>().Skip(1).First().Reference.GetName().ToLowerInvariant());
            Assert.AreEqual(OrderByDirection.Ascending, actual.Clauses.OfType<OrderByClause>().First().Direction);
            Assert.AreEqual(OrderByDirection.Ascending, actual.Clauses.OfType<OrderByClause>().Skip(1).First().Direction);
        }

        [Test]
        public void OrderByBarThenQuxxShouldBeAbleToMixOrdering()
        {
            var query = new SimpleQuery(null, "foo");
            query = query.OrderBy(new ObjectReference("bar"), OrderByDirection.Ascending)
                         .ThenBy(new ObjectReference("quux"), OrderByDirection.Descending);
            Assert.AreEqual("bar", query.Clauses.OfType<OrderByClause>().First().Reference.GetName());
            Assert.AreEqual("quux", query.Clauses.OfType<OrderByClause>().Skip(1).First().Reference.GetName());
            Assert.AreEqual(OrderByDirection.Ascending, query.Clauses.OfType<OrderByClause>().First().Direction);
            Assert.AreEqual(OrderByDirection.Descending, query.Clauses.OfType<OrderByClause>().Skip(1).First().Direction);
        }
        
        [Test]
        public void ThenByShouldModifyOrderAscending()
        {
            var query = new SimpleQuery(null, "foo");
            query = query.OrderBy(new ObjectReference("bar")).ThenBy(new ObjectReference("quux"));
            var actual = query.Clauses.OfType<OrderByClause>().ToArray();
            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("bar", actual[0].Reference.GetName());
            Assert.AreEqual(OrderByDirection.Ascending, actual[0].Direction);
            Assert.AreEqual("quux", actual[1].Reference.GetName());
            Assert.AreEqual(OrderByDirection.Ascending, actual[1].Direction);
        }

        [Test]
        public void OrderByDescendingShouldSetOrderDescending()
        {
            var query = new SimpleQuery(null, "foo");
            query = query.OrderByDescending(new ObjectReference("bar"));
            Assert.AreEqual("bar", query.Clauses.OfType<OrderByClause>().Single().Reference.GetName());
            Assert.AreEqual(OrderByDirection.Descending, query.Clauses.OfType<OrderByClause>().Single().Direction);
        }

        [Test]
        public void OrderByBarDescendingShouldSetOrderDescending()
        {
            dynamic query = new SimpleQuery(null, "foo");
            SimpleQuery actual = query.OrderByBarDescending();
            Assert.AreEqual("bar", actual.Clauses.OfType<OrderByClause>().Single().Reference.GetName().ToLowerInvariant());
            Assert.AreEqual(OrderByDirection.Descending, actual.Clauses.OfType<OrderByClause>().Single().Direction);
        }

        [Test]
        public void ThenByDescendingShouldModifyOrderAscending()
        {
            var query = new SimpleQuery(null, "foo");
            query = query.OrderBy(new ObjectReference("bar")).ThenByDescending(new ObjectReference("quux"));
            var actual = query.Clauses.OfType<OrderByClause>().ToArray();
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
            new SimpleQuery(null, "foo").ThenBy(new ObjectReference("bar"));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ThenByDescendingWithoutOrderByShouldThrow()
        {
            new SimpleQuery(null, "foo").ThenByDescending(new ObjectReference("bar"));
        }

        [Test]
        public void JoinShouldCreateExpression()
        {
            dynamic q = new SimpleQuery(null, "foo");
            q = q.Join(new ObjectReference("bar"), foo_id: new ObjectReference("id", new ObjectReference("foo")));
            var query = (SimpleQuery) q;
            Assert.AreEqual(1, query.Clauses.OfType<JoinClause>().Count());
            var join = query.Clauses.OfType<JoinClause>().Single();
            Assert.AreEqual("bar", join.Table.GetName());
        }

        [Test]
        public void JoinOnShouldSetAJoin()
        {
            dynamic db = new Database(null);
            dynamic quux;
            SimpleQuery q = db.foo.Query().Join(new ObjectReference("bar").As("quux"), out quux).On(db.foo.id == quux.foo_id);
            var join = q.Clauses.OfType<JoinClause>().Single();
            Assert.AreEqual("quux", join.Name);
            Assert.AreEqual(quux, join.Table);
            Assert.AreEqual(db.foo.id, join.JoinExpression.LeftOperand);
            Assert.AreEqual(quux.foo_id, join.JoinExpression.RightOperand);
            Assert.AreEqual(SimpleExpressionType.Equal, join.JoinExpression.Type);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ForUpdateWithoutSelectShouldThrow()
        {
            new SimpleQuery(null, "foo").ForUpdate(false);
        }

        [Test]
        public void ForUpdateShouldAddAClause()
        {
            var query = new SimpleQuery(null, "foo").Select(new AllColumnsSpecialReference()).ForUpdate(true);
            Assert.AreEqual(1, query.Clauses.OfType<ForUpdateClause>().Count());
            var forUpdate = query.Clauses.OfType<ForUpdateClause>().Single();
            Assert.IsTrue(forUpdate.SkipLockedRows);
        }

        [Test]
        public void SubsequentCallsToForUpdateShouldReplaceClause()
        {
            var query = new SimpleQuery(null, "foo").Select(new AllColumnsSpecialReference()).ForUpdate(false);
            Assert.AreEqual(1, query.Clauses.OfType<ForUpdateClause>().Count());
            var forUpdate = query.Clauses.OfType<ForUpdateClause>().Single();
            Assert.IsFalse(forUpdate.SkipLockedRows);
            query = query.ForUpdate(true);
            Assert.AreEqual(1, query.Clauses.OfType<ForUpdateClause>().Count());
            forUpdate = query.Clauses.OfType<ForUpdateClause>().Single();
            Assert.IsTrue(forUpdate.SkipLockedRows);
        }

        [Test]
        public void ClearForUpdateRemovesClause()
        {
            var query = new SimpleQuery(null, "foo").Select(new AllColumnsSpecialReference()).ForUpdate(false);
            Assert.AreEqual(1, query.Clauses.OfType<ForUpdateClause>().Count());
            var forUpdate = query.Clauses.OfType<ForUpdateClause>().Single();
            Assert.IsFalse(forUpdate.SkipLockedRows);
            query = query.ClearForUpdate();
            Assert.AreEqual(0, query.Clauses.OfType<ForUpdateClause>().Count());
        }
    }
}
