using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.InMemory
{
    static class ObjectMaths
    {
        public static object Increment(object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (value is int) return (int) value + 1;
            if (value is long) return (long) value + 1;
            if (value is short) return (short) value + 1;
            if (value is uint) return (uint)value + 1;
            if (value is ulong) return (ulong)value + 1;
            if (value is ushort) return (ushort)value + 1;
            if (value is decimal) return (decimal)value + 1;
            if (value is float) return (float)value + 1;
            if (value is double) return (double)value + 1;
            if (value is byte) return (byte)value + 1;
            if (value is sbyte) return (sbyte)value + 1;

            throw new ArgumentException("Cannot increment object type.");
        }
    }
}
