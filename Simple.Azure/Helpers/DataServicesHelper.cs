using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Simple.NExtLib;
using Simple.NExtLib.Xml.Syndication;
using Simple.NExtLib.IO;

namespace Simple.Azure.Helpers
{
    public static class DataServicesHelper
    {
        public static IEnumerable<IDictionary<string, object>> GetData(Stream stream)
        {
            return GetData(QuickIO.StreamToString(stream));
        }

        public static IEnumerable<IDictionary<string, object>> GetData(string text)
        {
            var feed = XElement.Parse(text);

            return feed.Descendants(null, "content").Select(content => GetData(content).ToIDictionary());
        }

        public static IEnumerable<KeyValuePair<string, object>> GetData(XElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            var properties = element.Element("m", "properties");

            if (properties == null) yield break;

            foreach (var property in properties.Elements())
            {
                yield return EdmHelper.Read(property);
            }
        }

        public static XElement CreateDataElement(IDictionary<string, object> row)
        {
            var entry = CreateEmptyEntryWithNamespaces();

            var properties = entry.Element(null, "content").Element("m", "properties");

            foreach (var prop in row)
            {
                EdmHelper.Write(properties, prop);
            }

            return entry;
        }

        private static XElement CreateEmptyEntryWithNamespaces()
        {
            var entry = XElement.Parse(Properties.Resources.DataServicesAtomEntryXml);
            entry.Element(null, "updated").SetValue(DateTime.UtcNow.ToIso8601String());
            return entry;
        }
    }
}
