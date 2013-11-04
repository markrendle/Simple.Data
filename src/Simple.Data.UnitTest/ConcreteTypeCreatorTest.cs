using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.UnitTest
{
    using NUnit.Framework;

    [TestFixture]
    public class ConcreteTypeCreatorTest
    {
        [Test]
        public void CanConvertDecimalToDouble()
        {
            var source = new Dictionary<string, object> { { "Value", 1.0m } };
            var target = ConcreteTypeCreator.Get(typeof (DecimalToDouble));
            object actual;
            Assert.IsTrue(target.TryCreate(source, out actual));
            Assert.IsInstanceOf<DecimalToDouble>(actual);
            Assert.AreEqual(1.0d, ((DecimalToDouble)actual).Value);
        }

        [Test]
        public void CanConvertDateTimeToNullableDateTime()
        {
            var expected = new DateTime(2011,9,8);
            var source = new Dictionary<string, object> { { "Value", expected } };
            var target = ConcreteTypeCreator.Get(typeof(DateTimeToNullableDateTime));
            object actual;
            Assert.IsTrue(target.TryCreate(source, out actual));
            Assert.IsInstanceOf<DateTimeToNullableDateTime>(actual);
            Assert.IsTrue(((DateTimeToNullableDateTime)actual).Value.HasValue);
            Assert.AreEqual(expected, ((DateTimeToNullableDateTime)actual).Value.Value);
        }

        [Test]
        public void CanConvertInt32ToEnum()
        {
            var expected = Int32ToEnum.Numbers.One;
            var source = new Dictionary<string, object> { { "Value", (int)expected } };
            var target = ConcreteTypeCreator.Get(typeof(Int32ToEnum));
            object actual;
            Assert.IsTrue(target.TryCreate(source, out actual));
            Assert.IsInstanceOf<Int32ToEnum>(actual);
            Assert.AreEqual(expected, ((Int32ToEnum)actual).Value);
        }
        
        [Test]
        public void CanConvertInt32ToNullableEnum()
        {
            Int32ToNullableEnum.Numbers? expected = Int32ToNullableEnum.Numbers.One;
            var source = new Dictionary<string, object> { { "Value", (int)expected } };
            var target = ConcreteTypeCreator.Get(typeof(Int32ToNullableEnum));
            object actual;
            Assert.IsTrue(target.TryCreate(source, out actual));
            Assert.IsInstanceOf<Int32ToNullableEnum>(actual);
            Assert.AreEqual(expected, ((Int32ToNullableEnum)actual).Value);
        }

        [Test]
        public void CanConvertStringToEnum()
        {
            var expected = Int32ToEnum.Numbers.One;
            var source = new Dictionary<string, object> { { "Value", expected.ToString() } };
            var target = ConcreteTypeCreator.Get(typeof(Int32ToEnum));
            object actual;
            Assert.IsTrue(target.TryCreate(source, out actual));
            Assert.IsInstanceOf<Int32ToEnum>(actual);
            Assert.AreEqual(expected, ((Int32ToEnum)actual).Value);
        }

        public class DecimalToDouble
        {
            public double Value { get; set; }
        }

        public class Int32ToNullableEnum
        {
            public Numbers? Value { get; set; }
            public enum Numbers
            {
                One = 1,
                Two = 2
            }
        }

        public class DateTimeToNullableDateTime
        {
            public DateTime? Value { get; set; }
        }

        public class Int32ToEnum
        {
            public enum Numbers
            {
                One = 1,
                Two = 2
            }

            public Numbers Value { get; set; }
        }
    }
}
