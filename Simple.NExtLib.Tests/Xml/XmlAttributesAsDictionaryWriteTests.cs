using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NExtLib.Unit;
using NUnit.Framework;
using Simple.NExtLib.Xml;

namespace Simple.NExtLib.Tests.Xml
{
    [TestFixture]
    public class XmlAttributesAsDictionaryWriteTests
    {
        [Test]
        public void PlainAttributeValueCheck()
        {
            var xml = new XmlElementAsDictionary("foo");
            xml.Attributes["bar"] = "quux";

            xml.ToElement().Attribute("bar").Value.ShouldEqual("quux");
        }

        [Test]
        public void DefaultNamespaceAttributeValueCheck()
        {
            var xml = new XmlElementAsDictionary("foo", "www.test.org");
            xml.Attributes["bar"] = "quux";

            xml.ToElement().Attribute(xml.ToElement().GetDefaultNamespace() + "bar").Value.ShouldEqual("quux");
        }

        [Test]
        public void PrefixedNamespaceAttributeValueCheck()
        {
            var xml = new XmlElementAsDictionary("foo");
            xml.AddPrefixedNamespace("q", "www.test.org");
            xml.Attributes["q:bar"] = "quux";

            xml.ToElement().Attribute(xml.ToElement().GetNamespaceOfPrefix("q") + "bar").Value.ShouldEqual("quux");
        }
    }
}
