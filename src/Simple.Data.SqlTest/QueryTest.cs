using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    using System.Collections;
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class QueryTest
    {
        public QueryTest()
        {
            DatabaseHelper.Reset();
        }

        [Fact]
        public async void CountWithNoCriteriaShouldSelectThree()
        {
            var db = DatabaseHelper.Open();
            var count = await db.Users.GetCount();
            Assert.AreEqual(3, count);
        }

        [Fact]
        public async void CountWithCriteriaShouldSelectTwo()
        {
            var db = DatabaseHelper.Open();
            int count = await db.Users.GetCount(db.Users.Age > 30);
            Assert.AreEqual(2, count);
        }

        [Fact]
        public async void CountByShouldSelectOne()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(1, await db.Users.GetCountByName("Bob"));
        }

       [Fact]
        public async void ExistsWithNoCriteriaShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(true, await db.Users.Exists());
        }

        [Fact]
        public async void ExistsWithValidCriteriaShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(true, await db.Users.Exists(db.Users.Age > 30));
        }

        [Fact]
        public async void ExistsWithInvalidCriteriaShouldReturnFalse()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(false, await db.Users.Exists(db.Users.Age == -1));
        }

        [Fact]
        public async void ExistsByValidValueShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(true, await db.Users.ExistsByName("Bob"));
        }

        [Fact]
        public async void ExistsByInvalidValueShouldReturnFalse()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(false, await db.Users.ExistsByName("Peter Kay"));
        }

        [Fact]
        public async void ColumnAliasShouldChangeDynamicPropertyName()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.Users.QueryById(1).Select(db.Users.Name.As("Alias")).First();
            Assert.AreEqual("Bob", actual.Alias);
        }

        [Fact]
        public async void MissingColumnShouldHaveColumnNotFoundMessage()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.Users.QueryById(1).Select(db.Users.Name).First();
            Assert.Throws<UnresolvableObjectException>(() => Console.WriteLine(actual.Bobbins), "Column not found.");
        }

        [Fact]
        public async void ShouldSelectFromOneToTen()
        {
            var db = DatabaseHelper.Open();
            var query = await db.PagingTest.QueryById(1.to(100)).Take(10).ToList();
            int index = 1;
            foreach (var row in query)
            {
                Assert.AreEqual(index, row.Id);
                index++;
            }
        }

        [Fact]
        public async void ShouldSelectFromElevenToTwenty()
        {
            var db = DatabaseHelper.Open();
            var query = await db.PagingTest.QueryById(1.to(100)).Skip(10).Take(10).ToList();
            int index = 11;
            foreach (var row in query)
            {
                Assert.AreEqual(index, row.Id);
                index++;
            }
        }

        [Fact]
        public async void ShouldSelectFromOneHundredToNinetyOne()
        {
            var db = DatabaseHelper.Open();
            var query = await db.PagingTest.QueryById(1.to(100)).OrderByDescending(db.PagingTest.Id).Skip(0).Take(10).ToList();
            int index = 100;
            foreach (var row in query)
            {
                Assert.AreEqual(index, row.Id);
                index--;
            }
        }

        //TODO: [Fact]
        public async void WithTotalCountShouldGiveCount()
        {
            Promise<int> count;
            var db = DatabaseHelper.Open();
            var list = await db.PagingTest.QueryById(1.to(50))
                .Take(10)
                .WithTotalCount(out count)
                .ToList();

            Assert.AreEqual(10, list.Count);
            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(50, count);
        }

        //TODO: [Fact]
        public async void WithTotalCountWithExplicitSelectShouldGiveCount()
        {
            Promise<int> count;
            var db = DatabaseHelper.Open();
            List<dynamic> list = await db.PagingTest.QueryById(1.to(50))
                .Select(db.PagingTest.Id)
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(50, count);
            Assert.AreEqual(10, list.Count);
            foreach (var dictionary in list.Cast<IDictionary<string,object>>())
            {
                Assert.AreEqual(1, dictionary.Count);
            }
        }

        //TODO: [Fact]
        public async void WithTotalCountWithExplicitSelectAndOrderByShouldGiveCount()
        {
            Promise<int> count;
            var db = DatabaseHelper.Open();
            List<dynamic> list = await db.PagingTest.QueryById(1.to(50))
                .Select(db.PagingTest.Id)
                .OrderByDescending(db.PagingTest.Id)
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(50, count);
            Assert.AreEqual(10, list.Count);
            foreach (var dictionary in list.Cast<IDictionary<string, object>>())
            {
                Assert.AreEqual(1, dictionary.Count);
            }
        }

        //TODO: [Fact]
// ReSharper disable InconsistentNaming
        public async void WithTotalCountShouldGiveCount_ObsoleteFutureVersion()
// ReSharper restore InconsistentNaming
        {
            Future<int> count;
            var db = DatabaseHelper.Open();
            var list = await db.PagingTest.QueryById(1.to(50))
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            Assert.AreEqual(10, list.Count);
            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(50, count);
        }

        [Fact]
        public async void ShouldDirectlyQueryDetailTable()
        {
            var db = DatabaseHelper.Open();
            var order = await db.Customers.QueryByNameAndAddress("Test", "100 Road").Orders.FirstOrDefault();
            Assert.IsNotNull(order);
            Assert.AreEqual(1, order.OrderId);
        }

        [Fact]
        public async void ShouldReturnNullWhenNoRowFound()
        {
            var db = DatabaseHelper.Open();
            string name = await db.Customers
                        .Query()
                        .Select(db.Customers.Name)
                        .Where(db.Customers.CustomerId == 0) // There is no CustomerId 0
                        .OrderByName()
                        .Take(1) // Should return only one record no matter what
                        .ToScalarOrDefault<string>();
            Assert.IsNull(name);
        }

        [Fact]
        public async void ToScalarListShouldReturnStringList()
        {
            var db = DatabaseHelper.Open();
            List<string> name = await db.Customers
                        .Query()
                        .Select(db.Customers.Name)
                        .OrderByName()
                        .ToScalarList<string>();
            Assert.IsNotNull(name);
            Assert.AreNotEqual(0, name.Count);
        }

        [Fact]
        public async void ToScalarArrayShouldReturnStringArray()
        {
            var db = DatabaseHelper.Open();
            string[] name = await db.Customers
                        .Query()
                        .Select(db.Customers.Name)
                        .OrderByName()
                        .ToScalarArray<string>();
            Assert.IsNotNull(name);
            Assert.AreNotEqual(0, name.Length);
        }

        [Fact]
        public async void HavingWithMinDateShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row = await
                db.GroupTestMaster.Query().Having(db.GroupTestMaster.GroupTestDetail.Date.Min() >=
                                                  new DateTime(2000, 1, 1))
                                                  .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("Two", row.Name);
        }
        [Fact]
        public async void HavingWithMaxDateShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row = await
                db.GroupTestMaster.Query().Having(db.GroupTestMaster.GroupTestDetail.Date.Max() <
                                                  new DateTime(2009, 1, 1))
                                                  .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("One", row.Name);
        }

        [Fact]
        public async void HavingWithCountShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row = await db.GroupTestMaster.Query()
                .Having(db.GroupTestMaster.GroupTestDetail.Id.Count() == 2)
                .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("Two", row.Name);
        }

        [Fact]
        public async void HavingWithAverageShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row = await db.GroupTestMaster.Query()
                .Having(db.GroupTestMaster.GroupTestDetail.Number.Average() == 2)
                .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("One", row.Name);
        }

        [Fact]
        public async void ToScalarOrDefault()
        {
            var db = DatabaseHelper.Open();
            int max = await db.Users.FindAllByName("ZXCVBNM").Select(db.Users.Age.Max()).ToScalarOrDefault<int>();
            Assert.AreEqual(0, max);
        }


        [Fact]
        public async void WithClauseShouldPreselectDetailTableAsCollection()
        {
            var db = DatabaseHelper.Open();
            var result = await db.Customers.FindAllByCustomerId(1).WithOrders().FirstOrDefault() as IDictionary<string, object>;
            Assert.IsNotNull(result);
            Assert.Contains("Orders", (ICollection)result.Keys);
            var orders = result["Orders"] as IList<IDictionary<string, object>>;
            Assert.IsNotNull(orders);
            Assert.AreEqual(1, orders.Count);
        }

        [Fact]
        public async void WithClauseShouldPreselectDetailTablesAsCollections()
        {
            var db = DatabaseHelper.Open();
            var result = await db.Customers.FindAllByCustomerId(1).WithOrders().WithNotes().FirstOrDefault() as IDictionary<string, object>;
            Assert.IsNotNull(result);
            Assert.Contains("Orders", (ICollection)result.Keys);
            var orders = result["Orders"] as IList<IDictionary<string, object>>;
            Assert.IsNotNull(orders);
            Assert.AreEqual(1, orders.Count);
            Assert.Contains("Notes", (ICollection)result.Keys);
            var notes = result["Notes"] as IList<IDictionary<string, object>>;
            Assert.IsNotNull(notes);
            Assert.AreEqual(2, notes.Count);
        }

        [Fact]
        public async void FindAllWithClauseWithJoinCriteriaShouldPreselectDetailTableAsCollection()
        {
            var db = DatabaseHelper.Open();
            var result = await db.Customers.FindAllByCustomerId(1).With(db.Customers.Orders.OrderItems).FirstOrDefault() as IDictionary<string, object>;
            Assert.IsNotNull(result);
            Assert.Contains("OrderItems", (ICollection)result.Keys);
            var orderItems = result["OrderItems"] as IList<IDictionary<string, object>>;
            Assert.IsNotNull(orderItems);
            Assert.AreEqual(1, orderItems.Count);
        }

        [Test, Ignore]
        public async void FindAllWithClauseWithNestedDetailTable()
        {
            var db = DatabaseHelper.Open();
            var result = await db.Customers.FindAllByCustomerId(1).With(db.Customers.Orders).With(db.Customers.Orders.OrderItems).FirstOrDefault() as IDictionary<string, object>;
            Assert.IsNotNull(result);
            Assert.Contains("Orders", result.Keys.ToArray());
            var orders = result["Orders"] as IList<IDictionary<string, object>>;
            Assert.IsNotNull(orders);
            Assert.AreEqual(1, orders.Count);
            var order = orders[0];
            Assert.Contains("OrderItems", order.Keys.ToArray());
        }

        [Fact]
        public async void GetWithClauseWithJoinCriteriaShouldPreselectDetailTableAsCollection()
        {
            var db = DatabaseHelper.Open();
            var result = await db.Customers.With(db.Customers.Orders.OrderItems).Get(1) as IDictionary<string, object>;
            Assert.IsNotNull(result);
            Assert.Contains("OrderItems", (ICollection)result.Keys);
            var orderItems = result["OrderItems"] as IList<IDictionary<string, object>>;
            Assert.IsNotNull(orderItems);
            Assert.AreEqual(1, orderItems.Count);
        }

        [Fact]
        public async void WithClauseWithTwoStepShouldPreselectManyToManyTableAsCollection()
        {
            var db = DatabaseHelper.Open();
            var result = await db.Customers.FindAll(db.Customers.Order.OrderId == 1).WithOrders().FirstOrDefault() as IDictionary<string, object>;
            Assert.IsNotNull(result);
            Assert.Contains("Orders", (ICollection)result.Keys);
            var orders = result["Orders"] as IList<IDictionary<string, object>>;
            Assert.IsNotNull(orders);
            Assert.AreEqual(1, orders.Count);
        }

        [Fact]
        public async void WithClauseShouldPreselectMasterTableAsDictionary()
        {
            var db = DatabaseHelper.Open();
            var result = await db.Orders.FindAllByOrderId(1).WithCustomer().FirstOrDefault() as IDictionary<string,object>;
            Assert.IsNotNull(result);
            Assert.Contains("Customer", (ICollection)result.Keys);
            var customer = result["Customer"] as IDictionary<string, object>;
            Assert.IsNotNull(customer);
        }

        [Fact]
        public async void WithClauseShouldCastToStaticTypeWithComplexProperty()
        {
            var db = DatabaseHelper.Open();
            Order actual = await db.Orders.FindAllByOrderId(1).WithCustomer().FirstOrDefault();
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Customer);
            Assert.AreEqual("Test", actual.Customer.Name);
            Assert.AreEqual("100 Road", actual.Customer.Address);
        }

        [Fact]
        public async void WithClauseShouldCastToStaticTypeWithCollection()
        {
            var db = DatabaseHelper.Open();
            Customer actual = await db.Customers.FindAllByCustomerId(1).WithOrders().FirstOrDefault();
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Orders.Single().OrderId);
            Assert.AreEqual(new DateTime(2010,10,10), actual.Orders.Single().OrderDate);
        }
        
        [Fact]
        public async void WithClauseShouldCastToStaticTypeWithEmptyCollection()
        {
            var db = DatabaseHelper.Open();
            var newCustomer = await db.Customers.Insert(Name: "No Orders");
            Customer actual = await db.Customers.FindAllByCustomerId(newCustomer.CustomerId).WithOrders().FirstOrDefault();
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Orders);
            Assert.AreEqual(0, actual.Orders.Count);
        }

        [Fact]
        public async void WithClauseContainingAliasShouldReturnResults()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.Customers
                           .With(db.Customers.Orders.As("Orders_1"))
                           .With(db.Customers.Orders.As("Orders_2"))
                           .FirstOrDefault();
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Orders_1.Single().OrderId);
            Assert.AreEqual(1, actual.Orders_2.Single().OrderId);
            Assert.AreEqual(new DateTime(2010, 10, 10), actual.Orders_1.Single().OrderDate);
            Assert.AreEqual(new DateTime(2010, 10, 10), actual.Orders_2.Single().OrderDate);
        }

        [Fact]
        public async void SelfJoinShouldNotThrowException()
        {
            var db = DatabaseHelper.Open();

            var q = db.Employees.Query().LeftJoin(db.Employees.As("Manager"), Id: db.Employees.ManagerId);
            q = q.Select(db.Employees.Name, q.Manager.Name.As("Manager"));
            List<dynamic> employees = await q.ToList();

            Assert.AreEqual(3, employees.Count); // The top man is missing

            var kingsSubordinates = employees.Where(e => e.Manager == "Alice").ToList();

            Assert.AreEqual(1, kingsSubordinates.Count);
        }
        
        [Fact]
        public async void OrderByOnJoinedColumnShouldUseJoinedColumn()
        {
            SimpleDataTraceSources.TraceSource.Switch.Level = SourceLevels.All;
            var traceListener = new TestTraceListener();
            SimpleDataTraceSources.TraceSource.Listeners.Add(traceListener);
            Trace.Listeners.Add(traceListener);
            var db = DatabaseHelper.Open();

            var q = db.Employees.Query().LeftJoin(db.Employees.As("Manager"), Id: db.Employees.ManagerId);
            q = q.Select(db.Employees.Name, q.Manager.Name.As("Manager"));
            List<dynamic> employees = await q.OrderBy(q.Manager.Name).ToList();
            SimpleDataTraceSources.TraceSource.Listeners.Remove(traceListener);
            Assert.Greater(traceListener.Output.IndexOf("order by [manager].[name]", StringComparison.OrdinalIgnoreCase), 0);
        }

        [Fact]
        public async void CanFetchMoreThanOneHundredRows()
        {
            var db = DatabaseHelper.Open();

            await db.Customers.Insert(Enumerable.Range(0, 200).Select(n => new Customer {Name = "Customer " + n}));

            List<dynamic> customers = await db.Customers.All().ToList();

            Assert.GreaterOrEqual(customers.Count, 200);
        }

        [Fact]
        public async void QueryWithForUpdateFalseShouldReturnCorrectResult()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.Users.QueryById(1).Select(db.Users.Name).ForUpdate(false).First();
            Assert.AreEqual("Bob", actual.Name);
        }

        [Fact]
        public async void QueryWithForUpdateTrueShouldReturnCorrectResult()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.Users.QueryById(1).Select(db.Users.Name).ForUpdate(true).First();
            Assert.AreEqual("Bob", actual.Name);
        }
    }
}
