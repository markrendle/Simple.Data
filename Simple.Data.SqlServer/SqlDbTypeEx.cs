using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlServer
{
    using System.Data;

    static class SqlDbTypeEx
    {
        private static readonly Dictionary<SqlDbType, Type> Map = new Dictionary<SqlDbType, Type>
                                                                      {
                                                                            { SqlDbType.BigInt, typeof(long)},
                                                                            { SqlDbType.Binary, typeof(byte[]) },
                                                                            { SqlDbType.Bit, typeof(bool)},
                                                                            { SqlDbType.Char, typeof(string)},
                                                                            { SqlDbType.Date, typeof(DateTime)},
                                                                            { SqlDbType.DateTime, typeof(DateTime)},
                                                                            { SqlDbType.DateTime2, typeof(DateTime)},
                                                                            { SqlDbType.DateTimeOffset, typeof(DateTime)},
                                                                            { SqlDbType.Decimal, typeof(decimal)},
                                                                            { SqlDbType.Float, typeof(double)},
                                                                            { SqlDbType.Image, typeof(byte[])},
                                                                            { SqlDbType.Int, typeof(int)},
                                                                            { SqlDbType.Money, typeof(decimal) },
                                                                            {SqlDbType.NChar, typeof(string)},
                                                                            { SqlDbType.NText, typeof(string)},
                                                                            { SqlDbType.NVarChar, typeof(string)},
                                                                            { SqlDbType.Real, typeof(Single)},
                                                                            { SqlDbType.SmallDateTime, typeof(DateTime)},
                                                                            { SqlDbType.SmallInt, typeof(short)},
                                                                            { SqlDbType.SmallMoney, typeof(decimal)},
                                                                            { SqlDbType.Structured, typeof(object)},
                                                                            { SqlDbType.Text, typeof(string)},
                                                                            { SqlDbType.Time, typeof(TimeSpan)},
                                                                            { SqlDbType.Timestamp, typeof(byte[])},
                                                                            { SqlDbType.TinyInt, typeof(byte)},
                                                                            { SqlDbType.Udt, typeof(object)},
                                                                            { SqlDbType.UniqueIdentifier, typeof(Guid)},
                                                                            { SqlDbType.VarBinary, typeof(byte[])},
                                                                            { SqlDbType.VarChar, typeof(string)},
                                                                            { SqlDbType.Variant, typeof(object)},
                                                                            { SqlDbType.Xml, typeof(string)}
                                                                      };
        public static Type ToClrType(this SqlDbType sqlDbType)
        {
            return Map[sqlDbType];
        }
    }
}
