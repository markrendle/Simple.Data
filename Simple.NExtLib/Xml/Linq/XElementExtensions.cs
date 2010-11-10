using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Xml.Linq
{
    /// <summary>
    /// Extension methods for <see cref="XElement"/>.
    /// </summary>
    public static class XElementExtensions
    {
        public static XElement Element(this XElement element, string prefix, string name)
        {
            return Elements(element, prefix, name).FirstOrDefault();
        }

        public static IEnumerable<XElement> Elements(this XElement element, string prefix, string name)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return element.Elements().Where(
                    x => x.Name.LocalName == name && element.GetPrefixOfNamespace(x.Name.Namespace) == null);
            }

            return element.Elements(ResolvePrefix(element, prefix) + name);
        }

        public static IEnumerable<XElement> Descendants(this XElement element, string prefix, string name)
        {
            var result = element.Descendants(ResolvePrefix(element, prefix) + name);

            if (result.Any()) return result;

            if (string.IsNullOrEmpty(prefix))
            {
                return element.Descendants().Where(
                    x => x.Name.LocalName == name && element.GetPrefixOfNamespace(x.Name.Namespace) == null);
            }

            return XElement.EmptySequence;
        }

        public static XAttribute Attribute(this XElement element, string prefix, string name)
        {
            return element.Attribute(ResolvePrefix(element, prefix) + name);
        }

        private static XNamespace ResolvePrefix(XElement element, string prefix)
        {
            return string.IsNullOrEmpty(prefix) ? element.GetDefaultNamespace() : element.GetNamespaceOfPrefix(prefix);
        }

        public static string ValueOrDefault(this XElement element)
        {
            return element == null ? string.Empty : element.Value;
        }
    }
}
