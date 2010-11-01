using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Simple.Data.Extensions;

namespace Simple.Data.Mocking
{
    static class XElementExtensions
    {
        public static XAttribute TryGetAttribute(this XElement element, XName attributeName)
        {
            var attribute = element.Attribute(attributeName) ??
                            element.Attributes().SingleOrDefault(
                                a => a.Name.ToString().Homogenize() == attributeName.ToString().Homogenize());
            return attribute;
        }

        public static string TryGetAttributeValue(this XElement element, XName attributeName)
        {
            var attribute = element.Attribute(attributeName) ??
                            element.Attributes().SingleOrDefault(
                                a => a.Name.ToString().Homogenize() == attributeName.ToString().Homogenize());
            return attribute == null ? null : attribute.Value;
        }

        public static IDictionary<string, object> AttributesToDictionary(this XElement element)
        {
            return element.Attributes().ToDictionary(a => a.Name.ToString(), a => ConvertValue(a,element.Parent));
        }

        private static object ConvertValue(XAttribute attribute, XElement tableElement)
        {
            var typeName = tableElement.Attribute(attribute.Name);
            if (typeName != null)
            {
                var type = Type.GetType(typeName.Value);
                if (type != null)
                {
                    return ConvertValue(attribute.Value, type);
                }
            }
            return attribute.Value;
        }

        private static object ConvertValue(string value, Type type)
        {
            var parseMethod = type.GetMethod("Parse", new Type[0]);
            if (parseMethod != null)
            {
                return parseMethod.Invoke(null, new[] {value});
            }
            return Convert.ChangeType(value, type);
        }
    }
}
