using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Simple.Data.Mocking
{
    public class XmlMockAdapter : IAdapter
    {
        private readonly Lazy<XElement> _data;

        public XElement Data
        {
            get { return _data.Value; }
        }

        public XmlMockAdapter(XElement element)
        {
            _data = new Lazy<XElement>(() => element);
        }

        public XmlMockAdapter(string xml)
        {
            _data = new Lazy<XElement>(() => XElement.Parse(xml));
        }

        public IDictionary<string, object> Find(string tableName, SimpleExpression criteria)
        {
            return FindAll(tableName, criteria).FirstOrDefault();
        }

        public IEnumerable<IDictionary<string, object>> FindAll(string tableName)
        {
            return GetTableElement(tableName).Elements().Select(e => e.AttributesToDictionary());
        }

        public IEnumerable<IDictionary<string, object>> FindAll(string tableName, IDictionary<string, object> criteria)
        {
            var query = GetTableElement(tableName).Elements();
            foreach (var criterion in criteria)
            {
                var column = criterion.Key;
                var value = criterion.Value;
                query = query.Where(xe => xe.TryGetAttributeValue(column).Equals(value.ToString()));
            }

            return query.Select(e => e.AttributesToDictionary());
        }

        public IEnumerable<IDictionary<string, object>> FindAll(string tableName, SimpleExpression criteria)
        {
            return
                GetTableElement(tableName).Elements().Where(GetPredicate(criteria)).Select(
                    e => e.AttributesToDictionary());
        }

        private static Func<XElement, bool> GetPredicate(SimpleExpression criteria)
        {
            if (criteria.Type == SimpleExpressionType.And || criteria.Type == SimpleExpressionType.Or)
            {
                var leftPredicate = GetPredicate((SimpleExpression) criteria.LeftOperand);
                var rightPredicate = GetPredicate((SimpleExpression)criteria.LeftOperand);
                return criteria.Type == SimpleExpressionType.And
                           ? new Func<XElement, bool>(xml => leftPredicate(xml) && rightPredicate(xml))
                           : new Func<XElement, bool>(xml => leftPredicate(xml) || rightPredicate(xml));
            }
            if (criteria.LeftOperand is DynamicReference)
            {
                var resolver = BuildReferenceResolver((DynamicReference)criteria.LeftOperand);
                return xml => resolver(xml) == criteria.RightOperand.ToString();
            }

            return xml => true;
        }

        private static Func<XElement, string> BuildReferenceResolver(DynamicReference reference)
        {
            var resolver = BuildElementResolver(reference);

            return xml => resolver(xml).TryGetAttributeValue(reference.Name);
        }

        private static Func<XElement, XElement> BuildElementResolver(DynamicReference reference)
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
            Func<XElement, XElement> resolver = xml => xml.Element(elementNames[1]);
            for (int i = 2; i < elementNames.Count - 1; i++)
            {
                var nested = resolver;
                var name = elementNames[i];
                resolver = xml => nested(xml).Element(name);
            }
            return resolver;
        }

        private XElement GetTableElement(string tableName)
        {
            var tableElement = Data.Element(tableName);
            if (tableElement == null) throw new UnresolvableObjectException(tableName);
            return tableElement;
        }


        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

        public int Update(string tableName, IDictionary<string, object> data, IDictionary<string, object> criteria)
        {
            throw new NotImplementedException();
        }

        public int Delete(string tableName, IDictionary<string, object> criteria)
        {
            throw new NotImplementedException();
        }
    }
}
