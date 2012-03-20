namespace Simple.Data
{
    using System.Collections.Generic;

    internal class DatabaseRunner : RunStrategy
    {
        private readonly Adapter _adapter;
        public DatabaseRunner(Adapter adapter)
        {
            _adapter = adapter;
        }

        internal override IDictionary<string, object> FindOne(string tableName, SimpleExpression criteria)
        {
            return _adapter.FindOne(tableName, criteria);
        }

        internal override int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList)
        {
            return _adapter.UpdateMany(tableName, dataList);
        }

        internal override int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames)
        {
            return _adapter.UpdateMany(tableName, dataList, criteriaFieldNames);
        }

        internal override int UpdateMany(string tableName, IList<IDictionary<string, object>> newValuesList, IList<IDictionary<string, object>> originalValuesList)
        {
            int count = 0;
            for (int i = 0; i < newValuesList.Count; i++)
            {
                count += Update(tableName, newValuesList[i], originalValuesList[i]);
            }
            return count;
        }

        internal override int Update(string tableName, IDictionary<string, object> newValuesDict, IDictionary<string, object> originalValuesDict)
        {
            SimpleExpression criteria = CreateCriteriaFromOriginalValues(tableName, newValuesDict, originalValuesDict);
            var changedValuesDict = CreateChangedValuesDict(newValuesDict, originalValuesDict);
            return _adapter.Update(tableName, changedValuesDict, criteria);
        }

        /// <summary>
        ///  Finds data from the specified "table".
        ///  </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The criteria. This may be <c>null</c>, in which case all records should be returned.</param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        internal override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return _adapter.Find(tableName, criteria);
        }

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        internal override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired)
        {
            return _adapter.Insert(tableName, data, resultRequired);
        }

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        internal override IEnumerable<IDictionary<string, object>> InsertMany(string tableName, IEnumerable<IDictionary<string, object>> data, ErrorCallback onError, bool resultRequired)
        {
            return _adapter.InsertMany(tableName, data, (dict, exception) => onError(new SimpleRecord(dict), exception), resultRequired);
        }

        /// <summary>
        ///  Updates the specified "table" according to specified criteria.
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The new values.</param><param name="criteria">The expression to use as criteria for the update operation.</param><returns>The number of records affected by the update operation.</returns>
        internal override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            return _adapter.Update(tableName, data, criteria);
        }

        public override IDictionary<string, object> Upsert(string tableName, IDictionary<string, object> dict, SimpleExpression criteriaExpression, bool isResultRequired)
        {
            return _adapter.Upsert(tableName, dict, criteriaExpression, isResultRequired);
        }

        public override IEnumerable<IDictionary<string, object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list, bool isResultRequired, ErrorCallback errorCallback)
        {
            return _adapter.UpsertMany(tableName, list, isResultRequired, (dict, exception) => errorCallback(new SimpleRecord(dict), exception));
        }

        public override IDictionary<string, object> Get(string tableName, object[] args)
        {
            return _adapter.Get(tableName, args);
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return _adapter.RunQuery(query, out unhandledClauses);
        }

        public override IEnumerable<IDictionary<string, object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list, IEnumerable<string> keyFieldNames, bool isResultRequired, ErrorCallback errorCallback)
        {
            return _adapter.UpsertMany(tableName, list, keyFieldNames, isResultRequired, (dict, exception) => errorCallback(new SimpleRecord(dict), exception));
        }

        /// <summary>
        ///  Deletes from the specified table.
        ///  </summary><param name="tableName">Name of the table.</param><param name="criteria">The expression to use as criteria for the delete operation.</param><returns>The number of records which were deleted.</returns>
        internal override int Delete(string tableName, SimpleExpression criteria)
        {
            return _adapter.Delete(tableName, criteria);
        }
    }
}