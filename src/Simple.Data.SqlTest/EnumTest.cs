using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlTest
{
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class EnumTest
    {
        public EnumTest()
        {
            DatabaseHelper.Reset();
        }

        [Fact]
        public async void ConvertsBetweenEnumAndInt()
        {
            var db = DatabaseHelper.Open();
            EnumTestClass actual = await db.EnumTest.Insert(Flag: TestFlag.One);
            Assert.AreEqual(TestFlag.One, actual.Flag);

            actual.Flag = TestFlag.Three;

            await db.EnumTest.Update(actual);

            actual = await db.EnumTest.FindById(actual.Id);
            Assert.AreEqual(TestFlag.Three, actual.Flag);
        }
    }

    class EnumTestClass
    {
        public int Id { get; set; }
        public TestFlag Flag { get; set; }
    }

    enum TestFlag
    {
        None = 0,
        One = 1,
        Two = 2, 
        Three = 3
    }
}
