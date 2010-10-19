using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.Ado;
using Simple.Data.Schema;

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

    }
}
