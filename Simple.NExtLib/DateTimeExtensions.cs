using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.NExtLib
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Formats the DateTime to the ISO 8601 standard, to maximum precision.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>String formatted like "2008-10-01T15:25:05.2852025Z"</returns>
        public static string ToIso8601String(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        }
    }
}
