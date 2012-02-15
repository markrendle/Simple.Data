using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public interface IAdapterWithTransactions
    {
        IAdapterTransaction BeginTransaction();
        IAdapterTransaction BeginTransaction(string name);

        /// <summary>
        ///  Finds data from the specified "table".
        ///  </summary>
        /// <param name="tableName">Name of the table.</param><param name="criteria">The criteria. This may be <c>null</c>, in which case all records should be returned.</param>
        /// <param name="transaction">The transaction with which the operation is associated.</param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria, IAdapterTransaction transaction);

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary>
        /// <param name="tableName">Name of the table.</param><param name="data">The values to insert.</param>
        /// <param name="transaction">The transaction with which the operation is associated.</param>
        /// <returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, IAdapterTransaction transaction, bool resultRequired);

        /// <summary>
        ///  Inserts many records into the specified "table".
        ///  </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="data">The list of records to insert.</param>
        /// <param name="transaction">The transaction with which the operation is associated.</param>
        /// <returns>If possible, return the newly inserted rows, including any automatically-set values such as primary keys or timestamps.</returns>
        IEnumerable<IDictionary<string, object>> InsertMany(string tableName, IEnumerable<IDictionary<string, object>> data, IAdapterTransaction transaction, Func<IDictionary<string,object>,Exception,bool> onError, bool resultRequired);

        /// <summary>
        ///  Updates the specified "table" according to specified criteria.
        ///  </summary>
        /// <param name="tableName">Name of the table.</param><param name="data">The new values.</param><param name="criteria">The expression to use as criteria for the update operation.</param>
        /// <param name="transaction">The transaction with which the operation is associated.</param>
        /// <returns>The number of records affected by the update operation.</returns>
        int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria, IAdapterTransaction transaction);

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
        int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IAdapterTransaction adapterTransaction);

        int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList, IAdapterTransaction adapterTransaction, IList<string> keyFields);

        int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames, IAdapterTransaction adapterTransaction);
        IDictionary<string, object> Upsert(string tableName, IDictionary<string, object> dict, SimpleExpression criteriaExpression, bool isResultRequired, IAdapterTransaction adapterTransaction);
        IEnumerable<IDictionary<string, object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list, IEnumerable<string> keyFieldNames, IAdapterTransaction adapterTransaction, bool isResultRequired, Func<IDictionary<string,object>,Exception,bool> errorCallback);
        IEnumerable<IDictionary<string, object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list, IAdapterTransaction adapterTransaction, bool isResultRequired, Func<IDictionary<string, object>, Exception, bool> errorCallback);
    }
}
