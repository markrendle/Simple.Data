using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Dynamic;

namespace Simple.Data
{
    internal static class DataRecordExtensions
    {
        public static dynamic ToDynamicRecord(this IDataRecord dataRecord)
        {
            return new DynamicRecord(dataRecord.ToDictionary());
        }

        public static Dictionary<string, object> ToDictionary(this IDataRecord dataRecord)
        {
            return dataRecord.GetFieldNames().ToDictionary(fieldName => fieldName, fieldName => dataRecord[fieldName]);
        }

        public static IEnumerable<string> GetFieldNames(this IDataRecord dataRecord)
        {
            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                yield return dataRecord.GetName(i);
            }
        }
    }
}
