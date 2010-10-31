using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.UnitTest
{
    [TestFixture]
    public class DynamicTableTest
    {
        [Test]
        public void PropertyShouldReturnDynamicReference()
        {
            //Arrange
            dynamic table = new DynamicTable("Test", null);

            // Act
            DynamicReference column = table.TestColumn;

            // Assert
            Assert.AreEqual("Test", column.Owner.Name);
            Assert.AreEqual("TestColumn", column.Name);
        }
    }
}
