namespace Shitty.Data
{
    using System.Collections.Generic;
    using System.Linq;

    internal abstract class RunStrategy
    {
        protected abstract Adapter Adapter { get; }

        /// <summary>
        ///  Finds data from the specified "table".
        ///  </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The criteria. This may be <c>null</c>, in which case all records should be returned.</param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        internal abstract IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria);

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        internal abstract IEnumerable<IDictionary<string, object>> InsertMany(string tableName, IEnumerable<IDictionary<string, object>> enumerable, ErrorCallback onError, bool resultRequired);

        /// <summary>
        ///  Inserts many records into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        internal abstract IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired);

        /// <summary>
        ///  Updates the specified "table" according to specified criteria.
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The new values.</param><param name="criteria">The expression to use as criteria for the update operation.</param><returns>The number of records affected by the update operation.</returns>
        internal abstract int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria);

        /// <summary>
        ///  Deletes from the specified table.
        ///  </summary><param name="tableName">Name of the table.</param><param name="criteria">The expression to use as criteria for the delete operation.</param><returns>The number of records which were deleted.</returns>
        internal abstract int Delete(string tableName, SimpleExpression criteria);

        internal abstract IDictionary<string, object> FindOne(string getQualifiedName, SimpleExpression criteriaExpression);

        internal abstract int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList);

        internal abstract int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames);

        internal abstract int UpdateMany(string tableName, IList<IDictionary<string, object>> newValuesList,
                                         IList<IDictionary<string, object>> originalValuesList);

        internal abstract int Update(string tableName, IDictionary<string, object> newValuesDict, IDictionary<string, object> originalValuesDict);

        public abstract IEnumerable<IDictionary<string, object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list, IEnumerable<string> keyFieldNames, bool isResultRequired, ErrorCallback errorCallback);

        public abstract IDictionary<string,object> Upsert(string tableName, IDictionary<string, object> dict, SimpleExpression criteriaExpression, bool isResultRequired);

        public abstract IEnumerable<IDictionary<string, object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list, bool isResultRequired, ErrorCallback errorCallback);

        public abstract IDictionary<string,object> Get(string tableName, object[] args);

        public abstract IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses);

        protected static Dictionary<string, object> CreateChangedValuesDict(
            IEnumerable<KeyValuePair<string, object>> newValuesDict, IDictionary<string, object> originalValuesDict)
        {
            var changedValuesDict =
                newValuesDict.Where(
                    kvp =>
                    (!originalValuesDict.ContainsKey(kvp.Key)) || !(Equals(kvp.Value, originalValuesDict[kvp.Key])))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return changedValuesDict;
        }

        protected SimpleExpression CreateCriteriaFromOriginalValues(string tableName,
                                                                           IDictionary<string, object> newValuesDict,
                                                                           IDictionary<string, object>
                                                                               originalValuesDict)
        {
            var criteriaValues = Adapter.GetKey(tableName, originalValuesDict);

            foreach (var kvp in originalValuesDict
                .Where(
                    originalKvp =>
                    newValuesDict.ContainsKey(originalKvp.Key) &&
                    !(Equals(newValuesDict[originalKvp.Key], originalKvp.Value))))
            {
                if (!criteriaValues.ContainsKey(kvp.Key))
                {
                    criteriaValues.Add(kvp);
                }
            };

            return ExpressionHelper.CriteriaDictionaryToExpression(tableName, criteriaValues);
        }
    }
}