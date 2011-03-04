using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.TestHelper;

namespace Simple.Data.Mysql40Test.SchemaTests
{
    [TestFixture]
    public class DatabaseSchemaTests : DatabaseSchemaTestsBase
    {
        private static readonly string ConnectionString =
            "server=localhost;user=SimpleData;database=SimpleDataTest;password=test;";

        protected override Database GetDatabase()
        {
            return Database.OpenConnection(ConnectionString);
        }

        
        [Test]
        public void TestTables()
        {
            //Mysql 4 on windows converts all table names to lower case.
            Assert.AreEqual(1, Schema.Tables.Count(t => t.ActualName == "users"));
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
            Assert.AreEqual("CustomerId", Schema.FindTable("Customers").PrimaryKey[0]);
        }

        [Test]
        public void TestForeignKey()
        {
            var foreignKey = Schema.FindTable("Orders").ForeignKeys.Single();
            Assert.AreEqual("customers", foreignKey.MasterTable.ToString());
            Assert.AreEqual("orders", foreignKey.DetailTable.ToString());
            Assert.AreEqual("CustomerId", foreignKey.Columns[0]);
            Assert.AreEqual("CustomerId", foreignKey.UniqueColumns[0]);
        }

        [Test]
        public void TestSingularResolution()
        {
            Assert.AreEqual("orderitems", Schema.FindTable("OrderItem").ActualName);
        }

        [Test]
        public void TestShoutySingularResolution()
        {
            Assert.AreEqual("orderitems",Schema.FindTable("ORDER_ITEM").ActualName);
        }

        [Test]
        public void TestShoutyPluralResolution()
        {
            Assert.AreEqual("orderitems", Schema.FindTable("ORDER_ITEM").ActualName);
        }
    }
}
