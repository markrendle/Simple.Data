using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System;

namespace Simple.Data.Extensions
{
    public static class HomogenizeEx
    {
        private static readonly ConcurrentDictionary<string, string> Cache
            = new ConcurrentDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly Regex HomogenizeRegex = new Regex("[^a-z0-9]");

        //help the compiler choose the Func<> call to GetOrAdd
        //This is what the compiler resolves with the inline method call
        //IL_000b:  call       string Simple.Data.Extensions.HomogenizeEx::HomogenizeImpl(string)
        //IL_0010:  callvirt   instance !1 class [mscorlib]System.Collections.Concurrent.ConcurrentDictionary`2<string,string>::GetOrAdd(!0,!1)

        /// <summary>
        /// Downshift a string and remove all non-alphanumeric characters.
        /// </summary>
        /// <param name="source">The original string.</param>
        /// <returns>The modified string.</returns>
        public static string Homogenize(this string source)
        {
            //we have to inline the call to force the compiler to not optimize for the string,string call.
            return source == null ? null : Cache.GetOrAdd(source, (notused) => {
                return string.Intern(HomogenizeRegex.Replace(source.ToLowerInvariant(), string.Empty));
            });
        }
    }
}