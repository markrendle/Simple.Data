using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Simple.Data.Mocking.Ado
{
    static class DataRowExtensions
    {
        public static IDictionary<string, object> ToDictionary(this DataRow dataRow)
        {
            return dataRow.Table.Columns.Cast<DataColumn>().ToDictionary(column => column.ColumnName, column => dataRow[column]);
        }
    }
}
