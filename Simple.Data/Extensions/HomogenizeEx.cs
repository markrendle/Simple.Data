using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System;

namespace Simple.Data.Extensions
{
    public static class HomogenizeEx
    {
        private static readonly ConcurrentDictionary<string, string> Cache
            = new ConcurrentDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        private static Regex _homogenizeRegex = new Regex("[^a-z0-9]");

        /// <summary>
        /// Downshift a string and remove all non-alphanumeric characters.
        /// </summary>
        /// <param name="source">The original string.</param>
        /// <returns>The modified string.</returns>
        public static string Homogenize(this string source)
        {
            return source == null ? null : Cache.GetOrAdd(source, HomogenizeImpl);
        }

        private static string HomogenizeImpl(string source)
        {
            return string.Intern(_homogenizeRegex.Replace(source.ToLowerInvariant(), string.Empty));
        }

        /// <summary>
        /// Sets the regular expression to be used for homogenizing object names.
        /// </summary>
        /// <param name="regex">A regular expression matching all non-comparing characters. The default is &quot;[^a-z0-9]&quot;.</param>
        /// <remarks>Homogenized strings are always forced to lower-case.</remarks>
        public static void SetRegularExpression(Regex regex)
        {
            _homogenizeRegex = regex;
        }
    }
}