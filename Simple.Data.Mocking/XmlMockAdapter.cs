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
            var tableElement = GetTableElement(tableName);
            if (tableElement == null)
            {
                tableElement = new XElement(tableName);
                Data.Add(tableElement);
            }
            var rowNameElement = tableElement.Elements().FirstOrDefault();
            var rowName = rowNameElement != null ? rowNameElement.Name : tableName + "_row";
            var element = new XElement(rowName);
            foreach (var kvp in data)
            {
                element.Add(new XAttribute(kvp.Key, kvp.Value));
            }
            tableElement.Add(element);
            return data;
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

        /// <summary>
        /// Deletes from the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The expression to use as criteria for the delete operation.</param>
        /// <returns>The number of records which were deleted.</returns>
        public int Delete(string tableName, SimpleExpression criteria)
        {
            var tableElement = GetTableElement(tableName);
            var elementsToDelete = GetTableElement(tableName).Elements()
                .Where(XmlPredicateBuilder.GetPredicate(criteria))
                .ToList();
            int deleted = elementsToDelete.Count;
            foreach (var element in elementsToDelete)
            {
                element.Remove();
            }
            return deleted;
        }

        public IEnumerable<string> GetKeyFieldNames(string tableName)
        {
            var keyAttribute = GetTableElement(tableName).Attribute("_keys");
            if (keyAttribute == null) return Enumerable.Empty<string>();
            return keyAttribute.Value.Split(',');
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