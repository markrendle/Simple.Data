using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public interface IAdapterWithRelation
    {
        /// <summary>
        /// Determines whether a relation is valid.
        /// </summary>
        /// <param name="tableName">Name of the known table.</param>
        /// <param name="relatedTableName">Name of the table to test.</param>
        /// <returns>
        /// 	<c>true</c> if there is a valid relation; otherwise, <c>false</c>.
        /// </returns>
        bool IsValidRelation(string tableName, string relatedTableName);

        /// <summary>
        /// Finds data from a "table" related to the specified "table".
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="row"></param>
        /// <param name="relatedTableName"></param>
        /// <param name="database"></param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        /// <remarks>When implementing the <see cref="Adapter"/> interface, if relationships are not possible, throw a <see cref="NotSupportedException"/>.</remarks>
        object FindRelated(string tableName, IDictionary<string, object> row, string relatedTableName, DataStrategy database);
    }
}
