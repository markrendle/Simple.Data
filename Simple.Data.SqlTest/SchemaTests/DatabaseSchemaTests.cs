using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.SqlTest.SchemaTests
{
    [TestFixture]
    public class DatabaseSchemaTests
    {
        private static DatabaseSchema GetSchema()
        {
            Database db = Database.OpenConnection(Properties.Settings.Default.ConnectionString);
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
            var columns = table.Columns.ToList();
            Assert.AreEqual(1, columns.Count(c => c.ActualName == "Id"));
        }

        [Test]
        public void TestPrimaryKey()
        {
            var schema = GetSchema();
            var table = schema.FindTable("Customers");
            Assert.AreEqual(1, table.PrimaryKey.Length);
            Assert.AreEqual("CustomerId", table.PrimaryKey[0]);
        }

        [Test]
        public void TestForeignKey()
        {
            var schema = GetSchema();
            var table = schema.FindTable("Orders");
            var fkey = table.ForeignKeys.Single();
            Assert.AreEqual("CustomerId", fkey.Columns[0]);
            Assert.AreEqual("Customers", fkey.MasterTable);
            Assert.AreEqual("CustomerId", fkey.UniqueColumns[0]);
        }

        [Test]
        public void TestIdentityIsTrueWhenItShouldBe()
        {
            var column = GetSchema().FindTable("Customers").FindColumn("CustomerId");
            Assert.IsTrue(column.IsIdentity);
        }

        [Test]
        public void TestIdentityIsFalseWhenItShouldBe()
        {
            var column = GetSchema().FindTable("Customers").FindColumn("Name");
            Assert.IsFalse(column.IsIdentity);
        }
    }
}
