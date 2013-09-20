using Simple.Data.Operations;

namespace Simple.Data
{
    using System.Collections.Generic;
    using System.Linq;

    internal class DatabaseRunner : RunStrategy
    {
        private readonly Adapter _adapter;
        public DatabaseRunner(Adapter adapter)
        {
            _adapter = adapter;
        }

        protected override Adapter Adapter
        {
            get { return _adapter; }
        }

        internal override IReadOnlyDictionary<string, object> FindOne(FindOperation operation)
        {
            return _adapter.FindOne(operation);
        }

        internal override int UpdateMany(string tableName, IList<IReadOnlyDictionary<string, object>> dataList)
        {
            return _adapter.UpdateMany(tableName, dataList);
        }

        internal override int UpdateMany(string tableName, IList<IReadOnlyDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames)
        {
            return _adapter.UpdateMany(tableName, dataList, criteriaFieldNames);
        }

        internal override int UpdateMany(string tableName, IList<IReadOnlyDictionary<string, object>> newValuesList, IList<IReadOnlyDictionary<string, object>> originalValuesList)
        {
            int count = 0;
            for (int i = 0; i < newValuesList.Count; i++)
            {
                count += Update(tableName, newValuesList[i], originalValuesList[i]);
            }
            return count;
        }

        internal override int Update(string tableName, IReadOnlyDictionary<string, object> newValuesDict, IReadOnlyDictionary<string, object> originalValuesDict)
        {
            SimpleExpression criteria = CreateCriteriaFromOriginalValues(tableName, newValuesDict, originalValuesDict);
            var changedValuesDict = CreateChangedValuesDict(newValuesDict, originalValuesDict);
            return _adapter.Update(tableName, changedValuesDict, criteria);
        }

        /// <summary>
        ///  Finds data from the specified "table".
        ///  </summary>
        /// <param name="operation">Operation details.</param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        internal override IEnumerable<IReadOnlyDictionary<string, object>> Find(FindOperation operation)
        {
            return _adapter.Find(operation);
        }

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        internal override IEnumerable<IReadOnlyDictionary<string, object>> Insert(InsertOperation operation)
        {
            return _adapter.Insert(operation);
        }

        /// <summary>
        ///  Updates the specified "table" according to specified criteria.
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The new values.</param><param name="criteria">The expression to use as criteria for the update operation.</param><returns>The number of records affected by the update operation.</returns>
        internal override int Update(string tableName, IReadOnlyDictionary<string, object> data, SimpleExpression criteria)
        {
            return _adapter.Update(tableName, data, criteria);
        }

        public override IEnumerable<IReadOnlyDictionary<string, object>> Upsert(UpsertOperation operation)
        {
            return _adapter.Upsert(operation);
        }

        public override IReadOnlyDictionary<string, object> Get(GetOperation operation)
        {
            return _adapter.Get(operation);
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return _adapter.RunQuery(query, out unhandledClauses);
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