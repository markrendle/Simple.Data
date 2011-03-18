using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    interface IAdapterWithTransactions
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
        IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, IAdapterTransaction transaction);

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

    	object Max(string tableName, string columnName, SimpleExpression criteria);

		object Min(string tableName, string columnName, SimpleExpression criteria);
    }
}
