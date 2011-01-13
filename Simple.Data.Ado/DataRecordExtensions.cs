using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Simple.Data.Ado
{
    internal static class DataRecordExtensions
    {
        public static dynamic ToDynamicRecord(this IDataRecord dataRecord)
        {
            return ToDynamicRecord(dataRecord, null, null);
        }

        public static dynamic ToDynamicRecord(this IDataRecord dataRecord, string tableName, Database database)
        {
            return new SimpleRecord(dataRecord.ToDictionary(), tableName, database);
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
