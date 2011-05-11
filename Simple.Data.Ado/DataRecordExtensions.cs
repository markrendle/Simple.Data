using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Simple.Data.Extensions;

namespace Simple.Data.Ado
{
    internal static class DataRecordExtensions
    {
        public static dynamic ToDynamicRecord(this IDataRecord dataRecord)
        {
            return ToDynamicRecord(dataRecord, null, null);
        }

        public static dynamic ToDynamicRecord(this IDataRecord dataRecord, IDictionary<string,int> index)
        {
            return ToDynamicRecord(dataRecord, index, null, null);
        }

        public static dynamic ToDynamicRecord(this IDataRecord dataRecord, string tableName, Database database)
        {
            return new SimpleRecord(dataRecord.ToDictionary(), tableName, database);
        }

        public static dynamic ToDynamicRecord(this IDataRecord dataRecord, IDictionary<string,int> index, string tableName, Database database)
        {
            return new SimpleRecord(dataRecord.ToDictionary(index), tableName, database);
        }

        public static IDictionary<string, object> ToDictionary(this IDataRecord dataRecord)
        {
            return dataRecord.ToDictionary(dataRecord.CreateDictionaryIndex());
//            return dataRecord.GetFieldNames().ToDictionary(fieldName => fieldName.Homogenize(), fieldName => DBNullToClrNull(dataRecord[fieldName]));
        }

        public static IDictionary<string, object> ToDictionary(this IDataRecord dataRecord, IDictionary<string,int> index)
        {
            return OptimizedDictionary.Create(index,dataRecord.GetValues());
        }

        public static Dictionary<string, int> CreateDictionaryIndex(this IDataRecord reader)
        {
            var keys =
                reader.GetFieldNames().Select((s, i) => new KeyValuePair<string, int>(s, i)).ToDictionary();
            return new Dictionary<string, int>(keys, HomogenizedEqualityComparer.DefaultInstance);
        }
        
        public static IEnumerable<string> GetFieldNames(this IDataRecord dataRecord)
        {
            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                yield return dataRecord.GetName(i);
            }
        }

        public static IEnumerable<object> GetValues(this IDataRecord dataRecord)
        {
            var values = new object[dataRecord.FieldCount];
            dataRecord.GetValues(values);
            return values.Replace(DBNull.Value, null);
        }

        private static object DBNullToClrNull(object value)
        {
            return value == DBNull.Value ? null : value;
        }
    }
}
