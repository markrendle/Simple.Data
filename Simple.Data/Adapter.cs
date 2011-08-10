using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data
{
    /// <summary>
    /// Provides a base class for adapters to persist data to data storage systems.
    /// Authors may derive from this class to create support for all kinds of databases or data stores.
    /// </summary>
    public abstract class Adapter
    {
        private readonly ExpandoObject _settings = new ExpandoObject();

        /// <summary>
        /// Gets an <see cref="ExpandoObject"/> with the settings for this <see cref="Adapter"/>.
        /// This property may be cast to an <see cref="IDictionary{TKey,TValue}"/> with a <see cref="string"/>
        /// key and <see cref="object"/> value for non-dynamic access.
        /// </summary>
        /// <value>The settings.</value>
        protected dynamic Settings
        {
            get { return _settings; }
        }

        public void Setup(object settings)
        {
            Setup(ObjectEx.ObjectToDictionary(settings));
        }

        public void Setup(IEnumerable<KeyValuePair<string,object>> settings)
        {
            var settingsAsDictionary = (IDictionary<string, object>) _settings;
            foreach (var keyValuePair in settings)
            {
                settingsAsDictionary.Add(keyValuePair);
            }

            OnSetup();
        }

        protected virtual void OnSetup()
        {
            
        }

        public virtual IDictionary<string,object> FindOne(string tableName, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  Finds data from the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="criteria">The criteria. This may be <c>null</c>, in which case all records should be returned.</param><returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        public abstract IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria);

        public abstract IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses);

        public virtual IObservable<IDictionary<string, object>> RunQueryAsObservable(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        public abstract IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data);

        /// <summary>
        /// Updates the specified "table" according to specified criteria.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="data">The new values.</param>
        /// <param name="criteria">The expression to use as criteria for the update operation.</param>
        /// <returns>The number of records affected by the update operation.</returns>
        public abstract int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria);

        /// <summary>
        ///  Deletes from the specified table.
        ///  </summary><param name="tableName">Name of the table.</param><param name="criteria">The expression to use as criteria for the delete operation.</param><returns>The number of records which were deleted.</returns>
        public abstract int Delete(string tableName, SimpleExpression criteria);

        /// <summary>
        ///  Gets the names of the fields which comprise the unique identifier for the specified table.
        ///  </summary><param name="tableName">Name of the table.</param><returns></returns>
//        public abstract IEnumerable<string> GetKeyFieldNames(string tableName);

        public virtual Func<object[],IDictionary<string,object>> CreateFindOneDelegate(string tableName, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        public virtual Func<object[], IEnumerable<IDictionary<string, object>>> CreateFindDelegate(string tableName, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IDictionary<string, object>> InsertMany(string tableName, IEnumerable<IDictionary<string, object>> data)
        {
            throw new NotImplementedException();
        }

        public virtual int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> data)
        {
            throw new NotImplementedException();
        }

        public virtual int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> data, IList<string> keyFields)
        {
            throw new NotImplementedException();
        }

        public abstract IEnumerable<IEnumerable<IDictionary<string, object>>> RunQueries(SimpleQuery[] queries, List<IEnumerable<SimpleQueryClauseBase>> unhandledClauses);

        public abstract bool IsExpressionFunction(string functionName, params object[] args);

        /// <summary>
        /// Updates the specified "table" according to default keys (to be handled by adapter).
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="data">The new values.</param>
        /// <returns>The number of records affected by the update operation.</returns>
        /// <remarks>For example, the Ado adapter will fulfil this functionality using Primary Key data.</remarks>
        public abstract int Update(string tableName, IDictionary<string, object> data);

        public virtual int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames)
        {
            int updatedCount = 0;
            var criteriaFieldNameList = criteriaFieldNames.ToList();
            foreach (var data in dataList)
            {
                updatedCount += Update(tableName, data, GetCriteria(tableName, criteriaFieldNameList, data));
            }
            return updatedCount;
        }

        protected static SimpleExpression GetCriteria(string tableName, IEnumerable<string> keyFieldNames, IDictionary<string, object> record)
        {
            var criteria = new Dictionary<string, object>();

            foreach (var keyFieldName in keyFieldNames)
            {
                var name = keyFieldName;
                var keyValuePair = record.Where(kvp => kvp.Key.Homogenize().Equals(name.Homogenize())).SingleOrDefault();
                if (string.IsNullOrWhiteSpace(keyValuePair.Key))
                {
                    throw new InvalidOperationException("Key field value not set.");
                }

                criteria.Add(keyFieldName, keyValuePair.Value);
                record.Remove(keyValuePair);
            }
            return ExpressionHelper.CriteriaDictionaryToExpression(tableName, criteria);
        }
    }
}
