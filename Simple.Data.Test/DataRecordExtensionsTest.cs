using System.Collections.Generic;
using Simple.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace Simple.Data.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for DataRecordExtensionsTest and is intended
    ///to contain all DataRecordExtensionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataRecordExtensionsTest
    {
        /// <summary>
        ///A test for ToDynamicRow
        ///</summary>
//        [TestMethod()]
        public void ToDynamicRowTest()
        {
            var data = new Dictionary<string, object>
                           {
                               { "Foo", 42 },
                               { "Bar", "P" },
                               { "Quux", null }
                           };
            //IDataRecord dataRecord = new DataRecordStub(data);
            //var row = dataRecord.ToDynamicRecord();
            //Assert.AreEqual(42, row.Foo);
            //Assert.AreEqual("P", row.Bar);
            //Assert.IsNull(row.Quux);
        }
    }
}
