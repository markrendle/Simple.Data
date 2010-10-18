using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Simple.Data.UnitTest
{
    [TestClass]
    public class DynamicTableTest
    {
        [TestMethod]
        public void PropertyShouldReturnDynamicColumn()
        {
            //Arrange
            dynamic table = new DynamicTable("Test", null);

            // Act
            DynamicColumn column = table.TestColumn;

            // Assert
            Assert.AreEqual("Test", column.TableName);
            Assert.AreEqual("TestColumn", column.Name);
        }
    }
}
