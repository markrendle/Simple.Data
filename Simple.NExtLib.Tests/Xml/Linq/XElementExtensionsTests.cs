using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Xml.Linq;
using NExtLib.Unit;

namespace Simple.NExtLib.Tests.Xml.Linq
{
    [TestFixture]
    public class XElementExtensionsTests
    {
        [Test]
        public void TestXElementWithDefaultNamespace()
        {
            var element = XElement.Parse(Properties.Resources.XmlWithDefaultNamespace);
            var list = element.Elements(null, "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element(null, "sub").Value.ShouldEqual("Foo");
            list[1].Element(null, "sub").Value.ShouldEqual("Bar");
        }

        [Test]
        public void TestXElementWithNoNamespace()
        {
            var element = XElement.Parse(Properties.Resources.XmlWithNoNamespace);
            var list = element.Elements(null, "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element(null, "sub").Value.ShouldEqual("Foo");
            list[1].Element(null, "sub").Value.ShouldEqual("Bar");
        }

        [Test]
        public void TestXElementWithPrefixedNamespace()
        {
            var element = XElement.Parse(Properties.Resources.XmlWithPrefixedNamespace);
            var list = element.Elements("c", "child").ToList();
            list.Count.ShouldEqual(2);
            list[0].Element("c", "sub").Value.ShouldEqual("Foo");
            list[1].Element("c", "sub").Value.ShouldEqual("Bar");
        }
    }
}
