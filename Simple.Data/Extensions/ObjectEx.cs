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
            if (obj == null) return new Dictionary<string, object>();
            return (from property in obj.GetType().GetProperties()
                                     select
                                         new KeyValuePair<string, object>(property.Name,
                                                                          property.GetValue(obj, null)))
                                                                          .ToDictionary();
        }
    }
}
