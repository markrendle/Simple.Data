using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Shitty.Data.Mocking.Ado
{
    static class DataTableExtensions
    {
        public static void AddColumns(this DataTable table, params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                table.Columns.Add(columnName);
            }
        }

        public static void AddRows(this DataTable table, IEnumerable<IEnumerable<object>> rows)
        {
            foreach (var row in rows)
            {
                table.Rows.Add(row.ToArray());
            }
        }
    }
}
