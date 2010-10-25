using System.Text.RegularExpressions;

namespace Simple.Data.Ado.Schema
{
    static class SchemaSpecificStringExtensions
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
    }
}
