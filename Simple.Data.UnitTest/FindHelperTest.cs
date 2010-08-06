using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Simple.Data.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for DynamicTableTest and is intended
    ///to contain all DynamicTableTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FindHelperTest
    {
        /// <summary>
        ///A test for GetFindByColumns
        ///</summary>
        [TestMethod()]
        public void GetFindByColumnsTest()
        {
            const string methodName = "NameAndPostcode";
            var actual = FindHelper.GetFindByColumns(new List<object> { "Bob", "SW1" }, methodName);
            Assert.AreEqual("name", actual[0], true);
            Assert.AreEqual("postcode", actual[1], true);
        }

        [TestMethod]
        public void GetFindBySqlTest()
        {
            const string methodName = "NameAndPostcode";
            var actual = FindHelper.GetFindBySql("foo", methodName, new List<object> { "Bob", "SW1" });
            Assert.AreEqual("select * from foo where name = ? and postcode = ?", actual, true);
        }
    }
}
