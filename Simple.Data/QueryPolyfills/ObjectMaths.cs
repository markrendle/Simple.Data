using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.QueryPolyfills
{
    static class ObjectMaths
    {
        public static object Increment(object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (value is int) return (int)value + 1;
            if (value is long) return (long)value + 1;
            if (value is short) return (short)value + 1;
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

        public static object Add(object value1, object value2)
        {
            if (ReferenceEquals(value1, null)) return value2;
            if (ReferenceEquals(value2, null)) return value1;

            // The order of these statements is important. Larger types are tested first.
            if (value1 is long) return (long)value1 + (long)value2;
            if (value1 is int) return (int)value1 + (int)value2;
            if (value1 is short) return (short)value1 + (short)value2;
            if (value1 is ulong) return (ulong)value1 + (ulong)value2;
            if (value1 is uint) return (uint)value1 + (uint)value2;
            if (value1 is ushort) return (ushort)value1 + (ushort)value2;
            if (value1 is double) return (double)value1 + (double)value2;
            if (value1 is float) return (float)value1 + (float)value2;
            if (value1 is decimal) return (decimal)value1 + (decimal)value2;
            if (value1 is byte) return (byte)value1 + (byte)value2;
            if (value1 is sbyte) return (sbyte)value1 + (sbyte)value2;

            throw new ArgumentException("Cannot add object types.");
        }

        public static object Divide(object value, int divisor)
        {
            if (value is long) return (long)value / divisor;
            if (value is int) return (int)value / divisor;
            if (value is short) return (short)value / divisor;
            if (value is ulong) return (ulong)value / (ulong)divisor;
            if (value is uint) return (uint)value / divisor;
            if (value is ushort) return (ushort)value / divisor;
            if (value is double) return (double)value / divisor;
            if (value is float) return (float)value / divisor;
            if (value is decimal) return (decimal)value / divisor;
            if (value is byte) return (byte)value / divisor;
            if (value is sbyte) return (sbyte)value / divisor;

            throw new ArgumentException("Cannot divide object type.");
        }
    }
}
