using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Schema
{
    static class SchemaSpecificStringExtensions
    {
        public static string Homogenize(this string source)
        {
            return source.ToLowerInvariant().Replace("_", "");
        }
    }
}
