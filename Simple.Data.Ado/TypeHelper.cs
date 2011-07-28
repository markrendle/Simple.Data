using System;
using System.Collections.Generic;

namespace Simple.Data.Ado
{
    using System.Data;

    static class TypeHelper
    {
        private static readonly HashSet<Type> BaseTypes = new HashSet<Type>
                                                              {
                                                                  typeof (bool),
                                                                  typeof (char),
                                                                  typeof (sbyte),
                                                                  typeof (byte),
                                                                  typeof (short),
                                                                  typeof (ushort),
                                                                  typeof (int),
                                                                  typeof (uint),
                                                                  typeof (long),
                                                                  typeof (ulong),
                                                                  typeof (float),
                                                                  typeof (double),
                                                                  typeof (decimal),
                                                                  typeof (DateTime),
                                                                  typeof (string),
                                                                  typeof (byte[]),
                                                                  typeof (Guid),
                                                              };
        
        public static bool IsKnownType(Type type)
        {
            return BaseTypes.Contains(type);
        }
    }
}