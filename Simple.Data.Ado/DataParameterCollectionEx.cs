using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    internal static class DataParameterCollectionEx
    {
        public static object GetValue(this IDataParameterCollection parameterCollection, string parameterName)
        {
            var parameter = parameterCollection[parameterName] as DbParameter;
            return parameter != null ? parameter.Value : null;
        }
    }
}
