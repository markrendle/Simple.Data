using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Simple.Data.Extensions
{
    static class SerializationInfoExtensions
    {
        public static T GetValue<T>(this SerializationInfo info, string name)
            where T : class
        {
            return info.GetValue(name, typeof (T)) as T;
        }
    }
}
