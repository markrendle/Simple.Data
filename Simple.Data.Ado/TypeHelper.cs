using System;
using System.Collections.Generic;

namespace Shitty.Data.Ado
{
    using System.Data;

    public static class TypeHelper
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
                                                                  typeof (DateTimeOffset),
                                                                  typeof (string),
                                                                  typeof (byte[]),
                                                                  typeof (Guid),
                                                              };

        private static readonly Dictionary<DbType, Type> DbTypeToClrTypeMap = new Dictionary<DbType, Type>
                                                                                  {
                                                                                      {DbType.Int16, typeof (short)},
                                                                                      {DbType.Int32, typeof (int)},
                                                                                      {DbType.Double, typeof (double)},
                                                                                      {DbType.Guid, typeof (Guid)},
                                                                                      {DbType.SByte, typeof (sbyte)},
                                                                                      {DbType.Single, typeof (Single)},
                                                                                      {DbType.Int64, typeof (long)},
                                                                                      {DbType.Object, typeof(object)},
                                                                                      {DbType.Byte, typeof(byte)},
                                                                                      {DbType.Boolean, typeof(bool)},
                                                                                      {DbType.AnsiString, typeof(string)},
                                                                                      {DbType.Binary, typeof(byte[])},
                                                                                      {DbType.DateTime, typeof(DateTime)},
                                                                                      {DbType.Decimal, typeof(decimal)},
                                                                                      {DbType.Currency, typeof(decimal)},
                                                                                      {DbType.Date, typeof(DateTime)},
                                                                                      {DbType.StringFixedLength, typeof(string)},
                                                                                      {DbType.AnsiStringFixedLength, typeof(string)},
                                                                                      {DbType.Xml, typeof(string)},
                                                                                      {DbType.DateTime2, typeof(DateTime)},
                                                                                      {DbType.VarNumeric, typeof(double)},
                                                                                      {DbType.UInt16, typeof(ushort)},
                                                                                      {DbType.String, typeof(string)},
                                                                                      {DbType.Time, typeof(TimeSpan)},
                                                                                      {DbType.UInt64, typeof(ulong)},
                                                                                      {DbType.UInt32, typeof(uint)},
                                                                                      {DbType.DateTimeOffset, typeof(DateTimeOffset)},
                                                                                  };
        
        public static bool IsKnownType(Type type)
        {
            return BaseTypes.Contains(type);
        }

        public static Type ToClrType(this DbType dbType)
        {
            if (!DbTypeToClrTypeMap.ContainsKey(dbType)) return typeof (object);
            return DbTypeToClrTypeMap[dbType];
        }
    }
}