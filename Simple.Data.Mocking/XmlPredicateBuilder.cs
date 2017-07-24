using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Shitty.Data.Mocking
{
    class XmlPredicateBuilder
    {
        public static Func<XElement, bool> GetPredicate(SimpleExpression criteria)
        {
            if (criteria.Type == SimpleExpressionType.And || criteria.Type == SimpleExpressionType.Or)
            {
                var leftPredicate = GetPredicate((SimpleExpression) criteria.LeftOperand);
                var rightPredicate = GetPredicate((SimpleExpression)criteria.RightOperand);
                return criteria.Type == SimpleExpressionType.And
                           ? (xml => leftPredicate(xml) && rightPredicate(xml))
                           : new Func<XElement, bool>(xml => leftPredicate(xml) || rightPredicate(xml));
            }
            if (criteria.LeftOperand is ObjectReference)
            {
                var resolver = BuildReferenceResolver((ObjectReference)criteria.LeftOperand);
                return xml => Equals(resolver(xml), criteria.RightOperand);
            }

            return xml => true;
        }

        private static Func<XElement, object> BuildReferenceResolver(ObjectReference reference)
        {
            var resolver = BuildElementResolver(reference);

            return xml => resolver(xml).TryGetAttribute(reference.GetName()).ConvertValue();
        }

        private static Func<XElement, XElement> BuildElementResolver(ObjectReference reference)
        {
            var elementNames = reference.GetAllObjectNames();
            if (elementNames.Length == 2)
            {
                return xml => xml;
            }

            return BuildNestedElementResolver(elementNames);
        }

        private static Func<XElement, XElement> BuildNestedElementResolver(IList<string> elementNames)
        {
            Func<XElement, XElement> resolver = xml => xml.Element(elementNames[1]).Elements().First();
            for (int i = 2; i < elementNames.Count - 1; i++)
            {
                var nested = resolver;
                var name = elementNames[i];
                resolver = xml => nested(xml).Element(name);
            }
            return resolver;
        }
    }
}
