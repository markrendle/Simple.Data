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
        public static dynamic ToDynamicRow(this IDataRecord dataRecord)
        {
            var expando = new ExpandoObject();
            var asDictionary = (IDictionary<string, object>)expando;

            foreach (var fieldName in dataRecord.GetFieldNames())
            {
                asDictionary[fieldName] = dataRecord[fieldName];
            }

            return expando;
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
