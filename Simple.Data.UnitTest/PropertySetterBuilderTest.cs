namespace Shitty.Data.UnitTest
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class PropertySetterBuilderTest
    {
        [Test]
        public void ConvertsInt32ToNullableEnum()
        {
            var actual = PropertySetterBuilder.SafeConvertNullable<TestInt32Enum>(1);
            Assert.True(actual.HasValue);
            Assert.AreEqual(TestInt32Enum.One, actual.Value);
        }
        
        [Test]
        public void ConvertsByteToNullableEnum()
        {
            const byte b = 1;
            var actual = PropertySetterBuilder.SafeConvertNullable<TestByteEnum>(b);
            Assert.True(actual.HasValue);
            Assert.AreEqual(TestByteEnum.One, actual.Value);
        }

        public enum TestInt32Enum
        {
            Zero = 0,
            One = 1,
            Two = 2
        }
        public enum TestByteEnum : byte
        {
            Zero = 0,
            One = 1,
            Two = 2
        }
    }
}
