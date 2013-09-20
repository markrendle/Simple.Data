using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Operations;

namespace Simple.Data
{
    public interface IAdapterWithTransactions
    {
        IAdapterTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        IAdapterTransaction BeginTransaction(string name, IsolationLevel isolationLevel = IsolationLevel.Unspecified);

        /// <summary>
        ///  Finds data from the specified "table".
        ///  </summary>
        /// <param name="operation">Operation details.</param>
        /// <param name="transaction">The transaction with which the operation is associated.</param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        IEnumerable<IReadOnlyDictionary<string, object>> Find(FindOperation operation, IAdapterTransaction transaction);

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary>
        /// <param name="operation"></param>
        /// <param name="transaction">The transaction with which the operation is associated.</param>
        /// <returns>If possible and required, return the newly inserted row(s), including any automatically-set values such as primary keys or timestamps.</returns>
        IEnumerable<IReadOnlyDictionary<string, object>> Insert(InsertOperation operation, IAdapterTransaction transaction);

        /// <summary>
        ///  Updates the specified "table" according to specified criteria.
        ///  </summary>
        /// <param name="tableName">Name of the table.</param><param name="data">The new values.</param><param name="criteria">The expression to use as criteria for the update operation.</param>
        /// <param name="transaction">The transaction with which the operation is associated.</param>
        /// <returns>The number of records affected by the update operation.</returns>
        int Update(string tableName, IReadOnlyDictionary<string, object> data, SimpleExpression criteria, IAdapterTransaction transaction);

        /// <summary>
        ///  Deletes from the specified table.
        ///  </summary>
        /// <param name="tableName">Name of the table.</param><param name="criteria">The expression to use as criteria for the delete operation.</param>
        /// <param name="transaction">The transaction with which the operation is associated.</param>
        /// <returns>The number of records which were deleted.</returns>
        int Delete(string tableName, SimpleExpression criteria, IAdapterTransaction transaction);

        /// <summary>
        ///  Updates the specified "table" according to primary key information where available.
        ///  </summary>
        /// <param name="tableName">Name of the table.</param><param name="dataList">The new values.</param>
        /// <param name="adapterTransaction">The transaction with which the operation is associated.</param>
        /// <returns>The number of records affected by the update operation.</returns>
        int UpdateMany(string tableName, IEnumerable<IReadOnlyDictionary<string, object>> dataList, IAdapterTransaction adapterTransaction);

        int UpdateMany(string tableName, IEnumerable<IReadOnlyDictionary<string, object>> dataList, IAdapterTransaction adapterTransaction, IList<string> keyFields);

        int UpdateMany(string tableName, IList<IReadOnlyDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames, IAdapterTransaction adapterTransaction);

        IEnumerable<IReadOnlyDictionary<string, object>> Upsert(UpsertOperation operation,
            IAdapterTransaction transaction);
        IReadOnlyDictionary<string, object> Get(GetOperation operation, IAdapterTransaction transaction);
        IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, IAdapterTransaction transaction, out IEnumerable<SimpleQueryClauseBase> unhandledClauses);
    }
}
