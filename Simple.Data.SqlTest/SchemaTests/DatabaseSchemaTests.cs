using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.TestHelper;

namespace Simple.Data.SqlTest.SchemaTests
{
    [TestFixture]
    public class DatabaseSchemaTests : DatabaseSchemaTestsBase
    {
        protected override Database GetDatabase()
        {
            return Database.OpenConnection(Properties.Settings.Default.ConnectionString);
        }

        [Test]
        public void TestTables()
        {
            Assert.AreEqual(1, Schema.Tables.Count(t => t.ActualName == "Users"));
        }

        [Test]
        public void TestColumns()
        {
            var table = Schema.FindTable("Users");
            Assert.AreEqual(1, table.Columns.Count(c => c.ActualName == "Id"));
        }

        [Test]
        public void TestPrimaryKey()
        {
            var table = Schema.FindTable("Customers");
            Assert.AreEqual(1, table.PrimaryKey.Length);
            Assert.AreEqual("CustomerId", table.PrimaryKey[0]);
        }

        [Test]
        public void TestForeignKey()
        {
            var table = Schema.FindTable("Orders");
            var fkey = table.ForeignKeys.Single();
            Assert.AreEqual("CustomerId", fkey.Columns[0]);
            Assert.AreEqual("Customers", fkey.MasterTable.Name);
            Assert.AreEqual("CustomerId", fkey.UniqueColumns[0]);
        }

        [Test]
        public void TestIdentityIsTrueWhenItShouldBe()
        {
            var column = Schema.FindTable("Customers").FindColumn("CustomerId");
            Assert.IsTrue(column.IsIdentity);
        }

        [Test]
        public void TestIdentityIsFalseWhenItShouldBe()
        {
            var column = Schema.FindTable("Customers").FindColumn("Name");
            Assert.IsFalse(column.IsIdentity);
        }
    }
}
