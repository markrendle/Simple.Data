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
            return FindTableWithExactName(tableName)
                   ?? FindTableWithCaseInsensitiveName(tableName)
                   ?? FindTableWithPluralName(tableName)
                   ?? FindTableWithSingularName(tableName);
        }

        private Table FindTableWithExactName(string tableName)
        {
            try
            {
                return this
                    .Where(t => t.ActualName.Equals(tableName))
                    .SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new AmbiguousObjectNameException(tableName);
            }
        }

        private Table FindTableWithCaseInsensitiveName(string tableName)
        {
            try
            {
                return this
                    .Where(t => t.ActualName.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))
                    .SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new AmbiguousObjectNameException(tableName);
            }
        }

        private Table FindTableWithPluralName(string tableName)
        {
            return FindTableWithCaseInsensitiveName(tableName.Pluralize());
        }

        private Table FindTableWithSingularName(string tableName)
        {
            if (tableName.IsPlural())
            {
                return FindTableWithCaseInsensitiveName(tableName.Singularize());
            }

            return null;
        }
    }
}
