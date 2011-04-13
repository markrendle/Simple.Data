using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Extensions
{
    public static class ObjectEx
    {
        public static IDictionary<string, object> ObjectToDictionary(this object obj)
        {
            return (from property in obj.GetType().GetProperties()
                                     select
                                         new KeyValuePair<string, object>(property.Name,
                                                                          property.GetValue(obj, null)))
                                                                          .ToDictionary();
        }

        public static IEnumerable<object> FlattenCriteriaArray(this IEnumerable<object> source)
        {
            return !source.Any(o => o is IRange)
                ? source
                : FlattenCriteriaArrayImpl(source);
        }

        private static IEnumerable<object> FlattenCriteriaArrayImpl(IEnumerable<object> source)
        {
            foreach (var o in source)
            {
                var range = o as IRange;
                if (range != null)
                {
                    yield return range.Start;
                    yield return range.End;
                }
                else
                {
                    yield return o;
                }
            }
        }
    }
}
