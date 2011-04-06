using System;
using System.Collections.Generic;

namespace Simple.Data
{
    public static class CommonTypes
    {
        private static readonly HashSet<Type> Set = new HashSet<Type>
                                                        {
                                                            typeof(Boolean),
                                                            typeof(Byte),
                                                            typeof(Char),
                                                            typeof(DateTime),
                                                            typeof(Decimal),
                                                            typeof(Double),
                                                            typeof(Guid),
                                                            typeof(Int16),
                                                            typeof(Int32),
                                                            typeof(Int64),
                                                            typeof(Single),
                                                            typeof(String),
                                                            typeof(UInt16),
                                                            typeof(UInt32),
                                                            typeof(UInt64),
                                                        };
        public static bool Contains(Type type)
        {
            return Set.Contains(type);
        }
    }
}