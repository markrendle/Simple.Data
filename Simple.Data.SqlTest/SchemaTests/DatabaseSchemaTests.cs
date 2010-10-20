using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.Ado;
using Simple.Data.Schema;

namespace Simple.Data.SqlTest.SchemaTests
{
    [TestClass]
    public class DatabaseSchemaTests
    {
        private static DatabaseSchema GetSchema()
        {
            Database db = Database.OpenConnection(Properties.Settings.Default.ConnectionString);
            return ((AdoAdapter)db.Adapter).GetSchema();
        }

        [TestMethod]
        public void TestTables()
        {
            var schema = GetSchema();
            Assert.AreEqual(1, schema.Tables.Count(t => t.ActualName == "Users"));
        }

        [TestMethod]
        public void TestColumns()
        {
            var schema = GetSchema();
            var table = schema.FindTable("Users");
            Assert.AreEqual(1, table.Columns.Count(c => c.ActualName == "Id"));
        }

        [TestMethod]
        public void TestPrimaryKey()
        {
            var schema = GetSchema();
            var table = schema.FindTable("Customers");
            Assert.AreEqual(1, table.PrimaryKey.Length);
            Assert.AreEqual("CustomerId", table.PrimaryKey[0]);
        }

        [TestMethod]
        public void TestForeignKey()
        {
            var schema = GetSchema();
            var table = schema.FindTable("Orders");
            var fkey = table.ForeignKeys.Single();
            Assert.AreEqual("CustomerId", fkey.Columns[0]);
            Assert.AreEqual("Customers", fkey.MasterTable);
            Assert.AreEqual("CustomerId", fkey.UniqueColumns[0]);
        }
    }
}
