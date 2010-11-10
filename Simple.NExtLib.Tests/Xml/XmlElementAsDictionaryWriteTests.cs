using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NExtLib.Unit;
using NUnit.Framework;
using System.Xml.Linq;
using System.Diagnostics;
using Simple.NExtLib.Xml;

namespace Simple.NExtLib.Tests.Xml
{
    [TestFixture]
    public class XmlElementAsDictionaryWriteTests
    {
        [Test]
        public void StringConstructorShouldCreateEmptyElementWithNoNamespace()
        {
            var actual = new XmlElementAsDictionary("foo");

            actual.ToElement().Name.LocalName.ShouldEqual("foo");
            actual.ToElement().Name.Namespace.ShouldEqual(XNamespace.None);
        }

        [Test]
        public void TwoStringConstructorShouldCreateEmptyElementWithNamespace()
        {
            var actual = new XmlElementAsDictionary("foo", "www.test.org");

            actual.ToElement().Name.LocalName.ShouldEqual("foo");
            actual.ToElement().Name.Namespace.NamespaceName.ShouldEqual("www.test.org");
        }

        [Test]
        public void TwoStringConstructorWithPrefixShouldCreateEmptyElementWithPrefixedNamespace()
        {
            var actual = new XmlElementAsDictionary("a:foo", "www.test.org");

            actual.ToElement().Name.LocalName.ShouldEqual("foo");
            actual.ToElement().Name.Namespace.ShouldEqual(actual.ToElement().GetNamespaceOfPrefix("a"));
            actual.Attributes["xmlns:a"].ShouldEqual("www.test.org");
        }

        [Test]
        public void UnsetAttributeShouldBeNull()
        {
            var actual = new XmlElementAsDictionary("foo");
            actual.Attributes["bar"].ShouldBeNull();
        }

        [Test]
        public void SetAttributeShouldBeSet()
        {
            var actual = new XmlElementAsDictionary("foo");
            actual.Attributes["bar"] = "Fnord";
            actual.Attributes["bar"].ShouldEqual("Fnord");
        }

        [Test]
        public void AttributeWithPrefixShouldHaveCorrectNamespace()
        {
            var actual = new XmlElementAsDictionary("foo");
            actual.AddPrefixedNamespace("x", "www.test.org");
            actual.Attributes["x:bar"] = "Fnord";

            var xname = actual.ToElement().GetNamespaceOfPrefix("x") + "bar";
            XAttribute attr;
            (attr = actual.ToElement().Attribute(xname)).ShouldNotBeNull();
            attr.Value.ShouldEqual("Fnord");
        }

        [Test]
        public void EmptyElementCreationWithClear()
        {
            var xml = new XmlElementAsDictionary("foo");
            xml["bar"].Clear();
            var actual = xml.ToElement();
            actual.ShouldNotBeNull();
            actual.Value.ShouldEqual(string.Empty);
        }

        [Test]
        public void CountShouldGetReflectNumberOfElements()
        {
            var xml = new XmlElementAsDictionary("foo");
            xml["bar"].Clear();

            xml.Count.ShouldEqual(1);

            xml.ToElement().Elements().Count().ShouldEqual(1);
        }
    }
}
