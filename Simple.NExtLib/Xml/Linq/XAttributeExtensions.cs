using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Xml.Linq
{
    public static class XAttributeExtensions
    {
        /// <summary>
        /// Returns the string value of the Attribute or <c>null</c> if the attribute is null.
        /// Null-safe, eliminates need for null-checking.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        public static string ValueOrDefault(this XAttribute attribute)
        {
            return attribute != null ? attribute.Value : null;
        }
    }
}
