using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.SqlCeTest.SchemaTests
{
    [TestClass]
    public class DatabaseSchemaTests
    {
        private static readonly string DatabasePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)),
            "TestDatabase.sdf");

        private static DatabaseSchema GetSchema()
        {
            Database db = Database.OpenFile(DatabasePath);
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
            Assert.AreEqual("CustomerId", schema.FindTable("Customers").PrimaryKey[0]);
        }

        [TestMethod]
        [Ignore]
        public void TestForeignKey()
        {
            var schema = GetSchema();
            var foreignKey = schema.FindTable("Orders").ForeignKeys.Single();
            Assert.AreEqual("Customers", foreignKey.MasterTable);
            Assert.AreEqual("Orders", foreignKey.DetailTable);
            Assert.AreEqual("CustomerId", foreignKey.Columns[0]);
            Assert.AreEqual("CustomerId", foreignKey.UniqueColumns[0]);
        }

        [TestMethod]
        public void TestSingularResolution()
        {
            Assert.AreEqual("OrderItems",
                GetSchema().FindTable("OrderItem").ActualName);
        }

        [TestMethod]
        public void TestShoutySingularResolution()
        {
            Assert.AreEqual("OrderItems",
                GetSchema().FindTable("ORDER_ITEM").ActualName);
        }

        [TestMethod]
        public void TestShoutyPluralResolution()
        {
            Assert.AreEqual("OrderItems",
                GetSchema().FindTable("ORDER_ITEM").ActualName);
        }
    }
}
