using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.SqlCe40Test.SchemaTests
{
    [TestFixture]
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

        [Test]
        public void TestTables()
        {
            var schema = GetSchema();
            Assert.AreEqual(1, schema.Tables.Count(t => t.ActualName == "Users"));
        }

        [Test]
        public void TestColumns()
        {
            var schema = GetSchema();
            var table = schema.FindTable("Users");
            Assert.AreEqual(1, table.Columns.Count(c => c.ActualName == "Id"));
        }

        [Test]
        public void TestPrimaryKey()
        {
            var schema = GetSchema();
            Assert.AreEqual("CustomerId", schema.FindTable("Customers").PrimaryKey[0]);
        }

        [Test]
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

        [Test]
        public void TestSingularResolution()
        {
            Assert.AreEqual("OrderItems",
                GetSchema().FindTable("OrderItem").ActualName);
        }

        [Test]
        public void TestShoutySingularResolution()
        {
            Assert.AreEqual("OrderItems",
                GetSchema().FindTable("ORDER_ITEM").ActualName);
        }

        [Test]
        public void TestShoutyPluralResolution()
        {
            Assert.AreEqual("OrderItems",
                GetSchema().FindTable("ORDER_ITEM").ActualName);
        }
    }
}
