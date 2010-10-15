using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Simple.Data.Mocking
{
    static class XElementExtensions
    {
        public static string TryGetAttributeValue(this XElement element, XName attributeName)
        {
            var attribute = element.Attribute(attributeName);
            return attribute == null ? null : attribute.Value;
        }

        public static IDictionary<string, object> AttributesToDictionary(this XElement element)
        {
            return element.Attributes().ToDictionary(a => a.Name.ToString(), a => (object)a.Value);
        }
    }
}
