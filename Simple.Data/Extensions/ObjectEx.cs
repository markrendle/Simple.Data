using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Extensions
{
    class ObjectEx
    {
        public static IDictionary<string, object> ObjectToDictionary(object obj)
        {
            return (from property in obj.GetType().GetProperties()
                                     select
                                         new KeyValuePair<string, object>(property.Name,
                                                                          property.GetValue(obj, null)))
                                                                          .ToDictionary();
        }
    }
}
