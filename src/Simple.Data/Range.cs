using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public static class Range
    {
        public static Range<T> to<T>(this T start, T end)
            where T : IComparable<T>
        {
            return new Range<T>(start, end);
        }

        public static IRange to(this string start, string end)
        {
            DateTime startDate, endDate;
            if (DateTime.TryParse(start, out startDate) && DateTime.TryParse(end, out endDate))
            {
                return new Range<DateTime>(startDate, endDate);
            }

            return new Range<string>(start, end);
        }
    }
}
