using System;
using NUnit.Framework;

namespace Simple.Data.SqlTest.SchemaTests
{
    [TestFixture]
    public class DataTypeTests
    {
        [Test]
        public void TestDecimalCanBeRetrievedCorrectly()
        {
            var db = DatabaseHelper.Open();
            var value = db.DecimalTest.FindById(1).Value;
            Assert.AreEqual(typeof(Decimal), value.GetType());
            Assert.AreEqual(1.234567, value);
        }

        [Test]
        public void TestDecimalCanBeInsertedCorrectly()
        {
            var db = DatabaseHelper.Open();
            var decimalTest = new { Value = 12.345678 };
            var value = db.DecimalTest.Insert(decimalTest).Value;
            Assert.AreEqual(typeof(Decimal), value.GetType());
            Assert.AreEqual(decimalTest.Value, value);
        }
    }
}
