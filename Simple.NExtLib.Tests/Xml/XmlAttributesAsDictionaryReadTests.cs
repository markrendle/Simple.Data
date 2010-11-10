using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Xml.Linq;
using Simple.NExtLib.Unit;
using Simple.NExtLib.Xml;

namespace Simple.NExtLib.Tests.Xml
{
    [TestFixture]
    public class XmlAttributesAsDictionaryReadTests
    {
        [Test]
        public void ReadWithNoNamespace()
        {
            var xml = new XmlElementAsDictionary(XElement.Parse(@"<foo bar=""quux""/>"));
            xml.Attributes["bar"].ShouldEqual("quux");
        }

        [Test]
        public void ReadWithDefaultNamespace()
        {
            var xml = new XmlElementAsDictionary(XElement.Parse(@"<foo bar=""quux"" xmlns=""www.test.org""/>"));
            xml.Attributes["bar"].ShouldEqual("quux");
        }

        [Test]
        public void ReadWithPrefixedNamespace()
        {
            var xml = new XmlElementAsDictionary(XElement.Parse(@"<foo xmlns:q=""www.test.org"" q:bar=""quux""/>"));
            xml.Attributes["q:bar"].ShouldEqual("quux");
        }
    }
}
