using System;
using System.Collections.Generic;

namespace Simple.Data.SqlCe40
{
    static class SqlTypeResolver
    {
        private static readonly Dictionary<string, Type> ClrTypes = new Dictionary<string, Type>
                                                                        {
                                                                            {"image", typeof (byte[])},
                                                                            {"text", typeof (string)},
                                                                            {"uniqueidentifier", typeof (Guid)},
                                                                            {"date", typeof (DateTime)},
                                                                            {"time", typeof (DateTime)},
                                                                            {"datetime2", typeof (DateTime)},
                                                                            {"datetimeoffset", typeof (DateTime)},
                                                                            {"tinyint", typeof (byte)},
                                                                            {"smallint", typeof (short)},
                                                                            {"int", typeof (int)},
                                                                            {"smalldatetime", typeof (DateTime)},
                                                                            {"real", typeof (float)},
                                                                            {"money", typeof (decimal)},
                                                                            {"datetime", typeof (DateTime)},
                                                                            {"float", typeof (float)},
                                                                            {"sql_variant", typeof (object)},
                                                                            {"ntext", typeof (string)},
                                                                            {"bit", typeof (bool)},
                                                                            {"decimal", typeof (decimal)},
                                                                            {"numeric", typeof (float)},
                                                                            {"smallmoney", typeof (decimal)},
                                                                            {"bigint", typeof (long)},
                                                                            {"varbinary", typeof (byte[])},
                                                                            {"varchar", typeof (string)},
                                                                            {"binary", typeof (byte[])},
                                                                            {"char", typeof (string)},
                                                                            {"timestamp", typeof (DateTime)},
                                                                            {"nvarchar", typeof (string)},
                                                                            {"nchar", typeof (string)},
                                                                            {"xml", typeof (string)},
                                                                            {"sysname", typeof (string)},

                                                                        };

        public static Type GetClrType(string sqlTypeName)
        {
            Type clrType;
            return ClrTypes.TryGetValue(sqlTypeName, out clrType) ? clrType : typeof (object);
        }
    }
}
