using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Xml.Linq;
using Simple.Azure.Helpers;
using System.Diagnostics;
using Simple.NExtLib.Xml;

namespace Simple.Azure.Tests
{
    [TestFixture]
    public class DataServicesHelperTests : AssertionHelper
    {
        [Test]
        public void Foo()
        {
            var element = XElement.Parse(Properties.Resources.QueryEntitiesResponseText).Element(null, "entry").Element(null, "content");
            DataServicesHelper.GetData(element);
        }

        [Test]
        public void CreateDataElement_string_test()
        {
            var dict = new Dictionary<string, object>
            {
                { "Foo", "bar" }
            };

            XElement xml = DataServicesHelper.CreateDataElement(dict);

            Assert.That(xml.Element(null, "content").Element("m", "properties").Element("d", "Foo").Value, Is.EqualTo("bar"));
        }

        [Test]
        public void CreateDataElement_DateTime_test()
        {
            DataTypeTestHelper(new DateTime(2006, 4, 9), EdmType.DateTime, "2006-04-09T00:00:00.0000000Z");
        }

        [Test]
        public void CreateDataElement_Boolean_true_test()
        {
            DataTypeTestHelper(true, EdmType.Boolean, "true");
        }

        [Test]
        public void CreateDataElement_Boolean_false_test()
        {
            DataTypeTestHelper(false, EdmType.Boolean, "false");
        }

        private void DataTypeTestHelper(object value, EdmType edmType, string formattedValue)
        {
            var dict = new Dictionary<string, object>
            {
                { "Foo", value }
            };

            XElement xml = DataServicesHelper.CreateDataElement(dict);

            var prop = xml.Element(null, "content").Element("m", "properties").Element("d", "Foo");

            Assert.That(prop.Attribute("m", "type").ValueOrDefault(), Is.EqualTo(edmType.ToString()));

            Assert.That(xml.Element(null, "content").Element("m", "properties").Element("d", "Foo").Value, Is.EqualTo(formattedValue));
        }
    }
}
