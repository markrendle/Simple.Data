using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Simple.NExtLib.Xml
{
    public static class XElementAsDictionaryExtension
    {
        public static XmlElementAsDictionary AsDictionary(this XElement element)
        {
            return new XmlElementAsDictionary(element);
        }

        public static XName ResolveName(this XElement element, string name)
        {
            if (name.Contains(':'))
            {
                var bits = name.Split(':');
                if (bits.Length != 2) throw new ArgumentException("name");

                var ns = element.GetNamespaceOfPrefix(bits[0]);
                if (ns == null) throw new ArgumentException("name");

                return ns + bits[1];
            }
            else if (element.GetDefaultNamespace() != null)
            {
                return element.GetDefaultNamespace() + name;
            }
            else
            {
                return name;
            }
        }

        public static string FormatName(this XElement element, XName name)
        {
            if (name.Namespace == null) return name.LocalName;
            if (name.Namespace == element.GetDefaultNamespace()) return name.LocalName;
            string prefix = element.GetPrefixOfNamespace(name.Namespace);
            if (!string.IsNullOrEmpty(prefix)) return prefix + ":" + name.LocalName;
            return name.ToString();
        }
    }
}
