using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.SqlCe;

namespace Simple.Data.SqlCe4Test
{
    /// <summary>
    /// Summary description for FindTests
    /// </summary>
    [TestClass]
    public class FindTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Trace.WriteLine(Environment.CurrentDirectory);
            var provider = ProviderHelper.GetProviderByFilename(@"..\..\..\TestDatabase.sdf");
            Assert.IsInstanceOfType(provider, typeof(SqlCeProvider));
        }

        [TestMethod]
        public void TestFindById()
        {
            Trace.WriteLine(Environment.CurrentDirectory);
            var db = Database.OpenFile(@"..\..\..\TestDatabase.sdf");
            var user = db.Users.FindById(1);
            Assert.AreEqual(1, user.Id);
        }
    }
}
