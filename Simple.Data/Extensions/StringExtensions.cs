using System;
using System.Linq;

namespace Simple.Data.Extensions
{
    static class StringExtensions
    {
        public static bool IsPlural(this string str)
        {
            return str.EndsWith("s", StringComparison.InvariantCultureIgnoreCase);
        }

        public static string Pluralize(this string str)
        {
            return string.Concat(str, str.IsAllUpperCase() ? "S" : "s");
        }

        public static string Singularize(this string str)
        {
            return str.EndsWith("s", StringComparison.InvariantCultureIgnoreCase) ? str.Substring(0, str.Length - 1) : str;
        }

        public static bool IsAllUpperCase(this string str)
        {
            return !str.Any(c => char.IsLower(c));
        }
    }
}
