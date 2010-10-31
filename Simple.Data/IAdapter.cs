using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    /// <summary>
    /// Provides a contract for adapters to persist data to data storage systems.
    /// Authors may implement this interface to create support for all kinds of databases or data stores.
    /// </summary>
    public interface IAdapter
    {
        /// <summary>
        /// Finds data from the specified "table".
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The criteria. This may be <c>null</c>, in which case all records should be returned.</param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria);

        /// <summary>
        /// Inserts a record into the specified "table".
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="data">The values to insert.</param>
        /// <returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data);

        /// <summary>
        /// Updates the specified "table".
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="data">The new values.</param>
        /// <param name="criteria">The values to use as criteria for the update.</param>
        /// <returns>The number of records affected by the update operation.</returns>
        int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria);

        /// <summary>
        /// Deletes from the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The values to use as criteria for the update.</param>
        /// <returns>The number of records which were deleted.</returns>
        int Delete(string tableName, IDictionary<string, object> criteria);
    }
}
