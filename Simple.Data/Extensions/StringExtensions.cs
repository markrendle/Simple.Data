using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Simple.Data.Extensions
{
    static class StringExtensions
    {
        private static readonly Regex HomogenizeRegex = new Regex("[^a-z0-9]");

        /// <summary>
        /// Downshift a string and remove all non-alphanumeric characters.
        /// </summary>
        /// <param name="source">The original string.</param>
        /// <returns>The modified string.</returns>
        public static string Homogenize(this string source)
        {
            return HomogenizeRegex.Replace(source.ToLowerInvariant(), string.Empty);
        }

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
