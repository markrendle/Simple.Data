using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Simple.Data.Schema
{
    class TableCollection : Collection<Table>
    {
        public TableCollection()
        {
        }

        public TableCollection(IEnumerable<Table> tables)
            : base(tables.ToList())
        {
        }

        /// <summary>
        /// Finds the Table with a name most closely matching the specified table name.
        /// This method will try an exact match first, then a case-insensitve search, then a pluralized or singular version.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A <see cref="Table"/> if a match is found; otherwise, <c>null</c>.</returns>
        public Table Find(string tableName)
        {
            return FindTableWithName(tableName.Homogenize())
                   ?? FindTableWithPluralName(tableName.Homogenize())
                   ?? FindTableWithSingularName(tableName.Homogenize());
        }

        private Table FindTableWithName(string tableName)
        {
            return this
                .Where(t => t.HomogenizedName.Equals(tableName))
                .SingleOrDefault();
        }

        private Table FindTableWithPluralName(string tableName)
        {
            return FindTableWithName(tableName.Pluralize());
        }

        private Table FindTableWithSingularName(string tableName)
        {
            if (tableName.IsPlural())
            {
                return FindTableWithName(tableName.Singularize());
            }

            return null;
        }
    }
}
