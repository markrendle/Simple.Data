using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Simple.Data.Mocking
{
    using System.Collections;
    using Extensions;

    public class XmlMockAdapter : Adapter, IAdapterWithRelation
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

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            if (criteria == null) return FindAll(tableName);
            return GetTableElement(tableName).Elements()
                .Where(XmlPredicateBuilder.GetPredicate(criteria))
                .Select(e => e.AttributesToDictionary());
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            unhandledClauses = Enumerable.Empty<SimpleQueryClauseBase>();
            var criteria = query.Clauses.OfType<WhereClause>().Aggregate(SimpleExpression.Empty,
                                                                            (seed, where) => seed && where.Criteria);
            return Find(query.TableName, criteria);
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
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

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            int updated = 0;
            var elementsToUpdate = GetTableElement(tableName).Elements()
                .Where(XmlPredicateBuilder.GetPredicate(criteria));

            foreach (var element in elementsToUpdate)
            {
                foreach (var kvp in data)
                {
                    var attribute = element.TryGetAttribute(kvp.Key);
                    if (attribute != null)
                    {
                        attribute.Value = kvp.Value.ToString();
                    }
                    else
                    {
                        element.SetAttributeValue(kvp.Key, kvp.Value);
                    }
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
        public override int Delete(string tableName, SimpleExpression criteria)
        {
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

        public override IEnumerable<IEnumerable<IDictionary<string, object>>> RunQueries(SimpleQuery[] queries, List<IEnumerable<SimpleQueryClauseBase>> unhandledClauses)
        {
            foreach (var query in queries)
            {
                IEnumerable<SimpleQueryClauseBase> unhandledClausesForThisQuery;
                var result = RunQuery(query, out unhandledClausesForThisQuery);
                unhandledClauses.Add(unhandledClausesForThisQuery);
                yield return result;
            }
        }

        public override bool IsExpressionFunction(string functionName, params object[] args)
        {
            return false;
        }

        public override int Update(string tableName, IDictionary<string, object> data)
        {
            return UpdateByKeyFields(tableName, data, GetKeyFieldNames(tableName));
        }

        internal int UpdateByKeyFields(string tableName, object entity, IEnumerable<string> keyFieldNames)
        {
            var record = ObjectToDictionary(entity);
            var list = record as IList<IDictionary<string, object>>;
            if (list != null) return UpdateMany(tableName, list, keyFieldNames);

            var dict = record as IDictionary<string, object>;
            var criteria = GetCriteria(tableName, keyFieldNames, dict);
            return Update(tableName, dict, criteria);
        }

        public override int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames)
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

        /// <summary>
        /// Determines whether a relation is valid.
        /// </summary>
        /// <param name="tableName">Name of the known table.</param>
        /// <param name="relatedTableName">Name of the table to test.</param>
        /// <returns>
        /// 	<c>true</c> if there is a valid relation; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidRelation(string tableName, string relatedTableName)
        {
            return GetTableElement(tableName).Elements().Any(e => e.Element(relatedTableName) != null);
        }

        /// <summary>
        /// Finds data from a "table" related to the specified "table".
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="row"></param>
        /// <param name="relatedTableName"></param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        /// <remarks>When implementing the <see cref="Adapter"/> interface, if relationships are not possible, throw a <see cref="NotSupportedException"/>.</remarks>
        public object FindRelated(string tableName, IDictionary<string, object> row, string relatedTableName)
        {
            var criteria = ExpressionHelper.CriteriaDictionaryToExpression(tableName, row);
            var relatedTableElement = GetTableElement(tableName).Elements()
                .Where(XmlPredicateBuilder.GetPredicate(criteria))
                .Single()
                .Element(relatedTableName);

            if (relatedTableElement != null && relatedTableElement.HasElements)
            {
                return relatedTableElement.Elements().Select(e => e.AttributesToDictionary());
            }

            return Enumerable.Empty<IDictionary<string, object>>();
        }

        private IEnumerable<string> GetKeyFieldNames(string tableName)
        {
            var keyAttribute = GetTableElement(tableName).Attribute("_keys");
            if (keyAttribute == null) return Enumerable.Empty<string>();
            return keyAttribute.Value.Split(',');
        }

        private static object ObjectToDictionary(object obj)
        {
            var dynamicRecord = obj as SimpleRecord;
            if (dynamicRecord != null)
            {
                return new Dictionary<string, object>(dynamicRecord, HomogenizedEqualityComparer.DefaultInstance);
            }

            var dictionary = obj as IDictionary<string, object>;
            if (dictionary != null)
            {
                return dictionary;
            }

            var list = obj as IEnumerable;
            if (list != null)
            {
                var originals = list.Cast<object>().ToList();
                var dictionaries = originals.Select(o => ObjectToDictionary(o) as IDictionary<string, object>).Where(o => o != null && o.Count > 0).ToList();
                if (originals.Count == dictionaries.Count)
                    return dictionaries;
            }

            return obj.ObjectToDictionary();
        }
    }
}