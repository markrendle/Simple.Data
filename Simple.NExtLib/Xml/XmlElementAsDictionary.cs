using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using Simple.NExtLib.IO;
using Simple.NExtLib.Linq;

namespace Simple.NExtLib.Xml
{
    public class XmlElementAsDictionary
    {
        private readonly XElement _element;

        public XmlElementAsDictionary(string name)
        {
            _element = new XElement(name);
        }

        public XmlElementAsDictionary(string name, string defaultNamespace)
        {
            _element = XElement.Parse(string.Format(@"<{0} xmlns{1}=""{2}""/>", name, GetPrefixPart(name), defaultNamespace));
        }

        public XmlElementAsDictionary(XElement element)
        {
            _element = element;
        }

        public XmlElementAsDictionary this[string name]
        {
            get
            {
                var xname = _element.ResolveName(name);

                return new XmlElementAsDictionary(_element.Element(xname) ?? CreateElement(xname));
            }
        }

        public IEnumerable<string> Keys
        {
            get { return _element.Elements().Select(element => _element.FormatName(element.Name)); }
        }

        public string Value
        {
            get
            {
                return _element.Value;
            }
            set
            {
                _element.Value = value;
            }
        }

        public void Clear()
        {
            _element.RemoveAll();
        }

        public int Count
        {
            get { return _element.Elements().Count(); }
        }

        private XElement CreateElement(XName name)
        {
            var element = new XElement(name);
            _element.Add(element);
            return element;
        }

        public override string ToString()
        {
            return _element.ToString();
        }

        public void AddPrefixedNamespace(string prefix, string @namespace)
        {
            _element.Add(new XAttribute(XNamespace.Xmlns + prefix, @namespace));
        }

        public XmlAttributesAsDictionary Attributes
        {
            get { return new XmlAttributesAsDictionary(_element); }
        }

        public XElement ToElement()
        {
            return new XElement(_element);
        }

        public XDocument ToDocument()
        {
            return new XDocument(_element);
        }

        public XDocument ToDocument(XDeclaration declaration)
        {
            return new XDocument(declaration, _element);
        }

        public bool ContainsKey(string name)
        {
            return ContainsKey(_element.ResolveName(name));
        }

        private bool ContainsKey(XName name)
        {
            return _element.Elements(name).Any();
        }

        public bool Remove(string name)
        {
            return Remove(_element.ResolveName(name));
        }

        private bool Remove(XName name)
        {
            var elementToRemove = _element.Element(name);

            if (elementToRemove == null) return false;

            elementToRemove.Remove();

            return true;
        }

        private static string GetPrefixPart(string name)
        {
            if (name.Contains(':'))
            {
                var bits = name.Split(':');
                if (bits.Length != 2) throw new ArgumentException("name");
                return ":" + bits[0];
            }

            return "";
        }

        public static XmlElementAsDictionary Parse(string text)
        {
            if (text == null) throw new ArgumentNullException("text");

            return new XmlElementAsDictionary(XElement.Parse(text));
        }

        public static XmlElementAsDictionary Parse(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            return Parse(QuickIO.StreamToString(stream));
        }

        public static IEnumerable<XmlElementAsDictionary> ParseDescendants(string text, string elementName)
        {
            var element = XElement.Parse(text);
            var xname = element.ResolveName(elementName);

            return element.Descendants(xname)
                .OrIfEmpty(element.Descendants(elementName))
                .Select(e => new XmlElementAsDictionary(e));
        }
    }
}
