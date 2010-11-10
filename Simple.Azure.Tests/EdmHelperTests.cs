// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Xml.Linq;
using Simple.Azure.Helpers;

namespace Simple.Azure.Tests
{
    [TestFixture]
    public class EdmHelperTests : AssertionHelper
    {
        const string Date = "2008-10-01T15:25:05.2852025Z";

        [Test]
        public void ReadEdmDateTime_parses_ISO8601()
        {
            var obj = EdmHelper.ReadEdmDateTime(Date);

            Assert.That(obj, Is.InstanceOf<DateTime>());

            var date = (DateTime)obj;

            Assert.That(date.Year, Is.EqualTo(2008));
            Assert.That(date.Month, Is.EqualTo(10));
            Assert.That(date.Day, Is.EqualTo(1));
            Assert.That(date.Hour, Is.EqualTo(16));
            Assert.That(date.Minute, Is.EqualTo(25));
            Assert.That(date.Second, Is.EqualTo(5));
        }

        [Test]
        public void Reads_EdmBoolean_true()
        {
            var kvp = EdmHelper.Read(TestElement("Boolean", "true"));
            Assert.That(kvp.Value, Is.InstanceOf<bool>());
            Assert.That((bool)kvp.Value, Is.True);
        }

        [Test]
        public void Reads_EdmBoolean_false()
        {
            var kvp = EdmHelper.Read(TestElement("Boolean", "false"));
            Assert.That(kvp.Value, Is.InstanceOf<bool>());
            Assert.That((bool)kvp.Value, Is.False);
        }

        [Test]
        public void Writes_EdmBoolean_true()
        {
            WriteTestHelper(true, EdmType.Boolean, "true");
        }

        [Test]
        public void Writes_EdmBoolean_false()
        {
            WriteTestHelper(false, EdmType.Boolean, "false");
        }

        [Test]
        public void Writes_EdmDateTime_with_correct_ISO_8601_formatting()
        {
            WriteTestHelper(DateTime.Parse(Date).ToUniversalTime(), EdmType.DateTime, Date);
        }

        [Test]
        public void Writes_EdmDouble_MinValue()
        {
            WriteTestHelper(double.MinValue, EdmType.Double, double.MinValue.ToString());
        }

        [Test]
        public void Writes_EdmDouble_MaxValue()
        {
            WriteTestHelper(double.MaxValue, EdmType.Double, double.MaxValue.ToString());
        }

        [Test]
        public void Writes_EdmGuid()
        {
            WriteTestHelper(Guid.Empty, EdmType.Guid, "00000000-0000-0000-0000-000000000000");
        }

        [Test]
        public void Writes_EdmInt32_MinValue()
        {
            WriteTestHelper(Int32.MinValue, EdmType.Int32, Int32.MinValue.ToString());
        }

        [Test]
        public void Writes_EdmInt32_MaxValue()
        {
            WriteTestHelper(Int32.MaxValue, EdmType.Int32, Int32.MaxValue.ToString());
        }

        [Test]
        public void Writes_EdmInt64_MinValue()
        {
            WriteTestHelper(Int64.MinValue, EdmType.Int64, Int64.MinValue.ToString());
        }

        [Test]
        public void Writes_EdmInt64_MaxValue()
        {
            WriteTestHelper(Int64.MaxValue, EdmType.Int64, Int64.MaxValue.ToString());
        }

        private static void WriteTestHelper(object value, EdmType edmType, string formattedValue)
        {
            var be = BaseTestElement();
            EdmHelper.Write(be, new KeyValuePair<string, object>("Test", value));

            var element = be.Element("d", "Test");

            Assert.That(element, Is.Not.Null);
            Assert.That(element.Attribute("m", "type").Value, Is.EqualTo(edmType.ToString()));
            Assert.That(element.Value, Is.EqualTo(formattedValue));
        }

        private static XElement BaseTestElement()
        {
            // Declare the d: and m: namespaces and qualify the elements
            return XElement.Parse(
@"<x xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices""
xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata""/>");
        }

        private static XElement TestElement(string type, string value)
        {
            // Declare the d: and m: namespaces and qualify the elements
            return XElement.Parse(string.Format(
@"<x xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices""
xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"">
<d:Test m:type=""Edm.{0}"">{1}</d:Test></x>",
                                            type, value)).Elements().First();
        }
    }
}
