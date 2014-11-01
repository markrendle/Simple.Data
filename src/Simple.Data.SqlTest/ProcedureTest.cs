using System.Diagnostics;

namespace Simple.Data.SqlTest
{
    using System.Data;
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class ProcedureTest
    {
        public ProcedureTest()
        {
            DatabaseHelper.Reset();
        }

        //TODO: [Fact]
        public async void GetCustomersTest()
        {
            var db = DatabaseHelper.Open();
            var results = await db.GetCustomers();
            var actual = results.First();
            Assert.AreEqual(1, actual.CustomerId);
        }

        //TODO: [Fact]
        public async void GetCustomerCountTest()
        {
            var db = DatabaseHelper.Open();
            var results = await db.GetCustomerCount();
            Assert.AreEqual(1, results.ReturnValue);
        }

        //TODO: [Fact]
        public async void FindGetCustomerCountAndInvokeTest()
        {
            var db = DatabaseHelper.Open();
            var getCustomerCount = db.GetCustomerCount;
            var results = await getCustomerCount();
            Assert.AreEqual(1, results.ReturnValue);
        }

        //TODO: [Fact]
        public async void FindGetCustomerCountUsingIndexerAndInvokeTest()
        {
            var db = DatabaseHelper.Open();
            var getCustomerCount = db["GetCustomerCount"];
            var results = await getCustomerCount();
            Assert.AreEqual(1, results.ReturnValue);
        }

        //TODO: [Fact]
        public async void SchemaUnqualifiedProcedureResolutionTest()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.SchemaProc().FirstOrDefault();
            Assert.IsNotNull(actual);
            Assert.AreEqual("dbo.SchemaProc", actual.Actual);
        }

        //TODO: [Fact]
        public async void SchemaQualifiedProcedureResolutionTest()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.test.SchemaProc().FirstOrDefault();
            Assert.IsNotNull(actual);
            Assert.AreEqual("test.SchemaProc", actual.Actual);
        }

        //TODO: [Fact]
        public async void GetCustomerCountAsOutputTest()
        {
            var db = DatabaseHelper.Open();
            var actual = await db.GetCustomerCountAsOutput();
            Assert.AreEqual(42, actual.OutputValues["Count"]);
        }

#if DEBUG // Trace is only written for DEBUG build
        //TODO: [Fact]
        public async void GetCustomerCountSecondCallExecutesNonQueryTest()
        {
            SimpleDataTraceSources.TraceSource.Switch.Level = SourceLevels.All;
            var listener = new TestTraceListener();
            SimpleDataTraceSources.TraceSource.Listeners.Add(listener);
            var db = DatabaseHelper.Open();
            await db.GetCustomerCount();
            Assert.IsFalse(listener.Output.Contains("ExecuteNonQuery"));
            await db.GetCustomerCount();
            Assert.IsTrue(listener.Output.Contains("ExecuteNonQuery"));
            SimpleDataTraceSources.TraceSource.Listeners.Remove(listener);
        }
#endif

        //TODO: [Fact]
        public async void GetCustomerAndOrdersTest()
        {
            var db = DatabaseHelper.Open();
            var results = await db.GetCustomerAndOrders(1);
            var customer = results.FirstOrDefault();
            Assert.IsNotNull(customer);
            Assert.AreEqual(1, customer.CustomerId);
            Assert.IsTrue(results.NextResult());
            var order = results.FirstOrDefault();
            Assert.IsNotNull(order);
            Assert.AreEqual(1, order.OrderId);
        }

		//TODO: [Fact]
		public async void AddCustomerTest()
		{
			var db = DatabaseHelper.Open();
			Customer customer;
			customer = await db.AddCustomer("Peter", "Address").FirstOrDefault();
			Assert.IsNotNull(customer);
			customer = await db.Customers.FindByCustomerId(customer.CustomerId);
			Assert.IsNotNull(customer);
		}

		//TODO: [Fact]
		public async void AddCustomerNullAddressTest()
		{
			var db = DatabaseHelper.Open();
			Customer customer;
			customer = await db.AddCustomer("Peter", null).FirstOrDefault();
			Assert.IsNotNull(customer);
			customer = await db.Customers.FindByCustomerId(customer.CustomerId);
			Assert.IsNotNull(customer);
		}

		//TODO: [Fact]
        public async void GetCustomerAndOrdersStillWorksAfterZeroRecordCallTest()
        {
            var db = DatabaseHelper.Open();
            db.GetCustomerAndOrders(1000);
            var results = await db.GetCustomerAndOrders(1);
            var customer = results.FirstOrDefault();
            Assert.IsNotNull(customer);
            Assert.AreEqual(1, customer.CustomerId);
            Assert.IsTrue(results.NextResult());
            var order = results.FirstOrDefault();
            Assert.IsNotNull(order);
            Assert.AreEqual(1, order.OrderId);
        }

        //TODO: [Fact]
        public async void ScalarFunctionIsCalledCorrectly()
        {
            var db = DatabaseHelper.Open();
            var results = await db.VarcharAndReturnInt("The answer to everything");
            Assert.AreEqual(42, results.ReturnValue);
        }

        //TODO: [Fact]
        public async void CallProcedureWithDataTable()
        {
            var db = DatabaseHelper.Open();
            var dataTable = new DataTable();
            dataTable.Columns.Add("Value");
            dataTable.Rows.Add("One");
            dataTable.Rows.Add("Two");
            dataTable.Rows.Add("Three");

            var actual = await db.ReturnStrings(dataTable).ToScalarList<string>();

            Assert.AreEqual(3, actual.Count);
            Assert.Contains("One", actual);
            Assert.Contains("Two", actual);
            Assert.Contains("Three", actual);
        }
    }
}
