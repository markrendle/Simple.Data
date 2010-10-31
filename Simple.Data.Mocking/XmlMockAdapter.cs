using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Simple.Data.Mocking
{
    public class XmlMockAdapter : IAdapter
    {
        private readonly Lazy<XElement> _data;

        public XmlMockAdapter(XElement element)
        {
            _data = new Lazy<XElement>(() => element);
        }

        public XmlMockAdapter(string xml)
        {
            _data = new Lazy<XElement>(() => XElement.Parse(xml));
        }

        public XElement Data
        {
            get { return _data.Value; }
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            if (criteria == null) return FindAll(tableName);
            return GetTableElement(tableName).Elements()
                .Where(XmlPredicateBuilder.GetPredicate(criteria))
                .Select(e => e.AttributesToDictionary());
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            throw new NotImplementedException();
        }

        public int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            int updated = 0;
            var elementsToUpdate = GetTableElement(tableName).Elements()
                .Where(XmlPredicateBuilder.GetPredicate(criteria));

            foreach (var element in elementsToUpdate)
            {
                foreach (var kvp in data)
                {
                    element.SetAttributeValue(kvp.Key, kvp.Value);
                }
                updated++;
            }

            return updated;
        }

        public int Delete(string tableName, IDictionary<string, object> criteria)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<IDictionary<string, object>> FindAll(string tableName)
        {
            return GetTableElement(tableName).Elements().Select(e => e.AttributesToDictionary());
        }

        private XElement GetTableElement(string tableName)
        {
            XElement tableElement = Data.Element(tableName);
            if (tableElement == null) throw new UnresolvableObjectException(tableName);
            return tableElement;
        }
    }
}