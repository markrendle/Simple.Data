using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Simple.Data.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for DynamicTableTest and is intended
    ///to contain all DynamicTableTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DynamicTableTest
    {
        /// <summary>
        ///A test for GetFindByColumns
        ///</summary>
        [TestMethod()]
        public void GetFindByColumnsTest()
        {
            const string methodName = "NameAndPostcode";
            var actual = DynamicTable.GetFindByColumns(new List<object> { "Bob", "SW1" }, methodName);
            Assert.AreEqual("name", actual[0]);
            Assert.AreEqual("postcode", actual[1]);
        }

        [TestMethod]
        public void GetFindBySqlTest()
        {
            const string methodName = "NameAndPostcode";
            var actual = DynamicTable.GetFindBySql("foo", methodName, new List<object> { "Bob", "SW1" });
            Assert.AreEqual("select * from foo where name = ? and postcode = ?", actual);
        }
    }
}
