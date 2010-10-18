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

        public IDictionary<string, object> Find(string tableName, IDictionary<string, object> criteria)
        {
            return FindAll(tableName, criteria).FirstOrDefault();
        }

        public IDictionary<string, object> Find(string tableName, SimpleExpression criteria)
        {
            throw new NotImplementedException();
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
