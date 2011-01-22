using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Simple.Data.Extensions;

namespace Simple.Data.Ado.Schema
{
    class ColumnCollection : Collection<Column>
    {
        public ColumnCollection()
        {
            
        }

        public ColumnCollection(IEnumerable<Column> columns) : base(columns.ToList())
        {
            
        }
        /// <summary>
        /// Finds the column with a name most closely matching the specified column name.
        /// This method will try an exact match first, then a case-insensitve search, then a pluralized or singular version.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>A <see cref="Column"/> if a match is found; otherwise, <c>null</c>.</returns>
        public Column Find(string columnName)
        {
            var column = FindColumnWithName(columnName);
            if (column == null) throw new UnresolvableObjectException(columnName);
            return column;
        }

        private Column FindColumnWithName(string columnName)
        {
            columnName = columnName.Homogenize();
            return this
                .Where(c => c.HomogenizedName.Equals(columnName))
                .SingleOrDefault();
        }
    }
}
