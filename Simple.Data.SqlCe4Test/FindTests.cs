using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.SqlCe;
using System.IO;

namespace Simple.Data.SqlCeTest
{
    /// <summary>
    /// Summary description for FindTests
    /// </summary>
    [TestClass]
    public class FindTests
    {
        private static readonly string DatabasePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)),
            "TestDatabase.sdf");

        [TestMethod]
        public void TestMethod1()
        {
            Trace.WriteLine(Environment.CurrentDirectory);
            var provider = ProviderHelper.GetProviderByFilename(DatabasePath);
            Assert.IsInstanceOfType(provider, typeof(SqlCeProvider));
        }

        [TestMethod]
        public void TestFindById()
        {
            Trace.WriteLine(Assembly.GetExecutingAssembly().CodeBase);
            Trace.WriteLine(Environment.CurrentDirectory);
            var db = Database.OpenFile(DatabasePath);
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }
    }
}
