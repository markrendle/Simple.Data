using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Azure.Tests
{
    [TestFixture]
    public class EdmTypeTests : AssertionHelper
    {
        [Test]
        public void FromSystemType_byte_array_returns_EdmBinary()
        {
            Assert.That(EdmType.FromSystemType(typeof(byte[])), Is.EqualTo(EdmType.Binary));
        }

        [Test]
        public void FromSystemType_boolean_returns_EdmBoolean()
        {
            Assert.That(EdmType.FromSystemType(typeof(bool)), Is.EqualTo(EdmType.Boolean));
        }

        [Test]
        public void FromSystemType_DateTime_returns_EdmDateTime()
        {
            Assert.That(EdmType.FromSystemType(typeof(DateTime)), Is.EqualTo(EdmType.DateTime));
        }

        [Test]
        public void FromSystemType_double_returns_EdmDouble()
        {
            Assert.That(EdmType.FromSystemType(typeof(double)), Is.EqualTo(EdmType.Double));
        }

        [Test]
        public void FromSystemType_Guid_returns_EdmGuid()
        {
            Assert.That(EdmType.FromSystemType(typeof(Guid)), Is.EqualTo(EdmType.Guid));
        }

        [Test]
        public void FromSystemType_int_returns_EdmInt32()
        {
            Assert.That(EdmType.FromSystemType(typeof(int)), Is.EqualTo(EdmType.Int32));
        }

        [Test]
        public void FromSystemType_long_returns_EdmInt64()
        {
            Assert.That(EdmType.FromSystemType(typeof(long)), Is.EqualTo(EdmType.Int64));
        }

        [Test]
        public void FromSystemType_string_returns_EdmString()
        {
            Assert.That(EdmType.FromSystemType(typeof(string)), Is.EqualTo(EdmType.String));
        }
    }
}
