using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.NExtLib
{
    public static class StringExtensions
    {
        public static string EnsureStartsWith(this string source, string value)
        {
            return (source == null || source.StartsWith(value)) ? source : value + source;
        }
    }
}
