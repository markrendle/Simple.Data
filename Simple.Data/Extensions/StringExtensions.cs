using System;
using System.Collections.Generic;
using System.Linq;

namespace Shitty.Data.Extensions
{
    public static class StringExtensions
    {
        public static bool IsPlural(this string str)
        {
            return _pluralizer.IsPlural(str);
        }

        public static string Pluralize(this string str)
        {
            return _pluralizer.Pluralize(str);
        }

        public static string Singularize(this string str)
        {
            return _pluralizer.Singularize(str);
        }

        public static bool IsAllUpperCase(this string str)
        {
            return !str.Any(char.IsLower);
        }

        public static string NullIfWhitespace(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }

        public static string OrDefault(this string str, string defaultValue)
        {
            return str ?? defaultValue;
        }

        private static IPluralizer _pluralizer = new SimplePluralizer();

        internal static void SetPluralizer(IPluralizer pluralizer)
        {
            _pluralizer = pluralizer ?? new SimplePluralizer();
        }
    }

    class SimplePluralizer : IPluralizer
    {
        public bool IsSingular(string word)
        {
            return !IsPlural(word);
        }

        public bool IsPlural(string word)
        {
            return word.EndsWith("s", StringComparison.InvariantCultureIgnoreCase);
        }

        public string Pluralize(string word)
        {
            return string.Concat(word, word.IsAllUpperCase() ? "S" : "s");
        }

        public string Singularize(string word)
        {
            return word.EndsWith("s", StringComparison.InvariantCultureIgnoreCase) ? word.Substring(0, word.Length - 1) : word;
        }
        
    }
}
