namespace Simple.Data.SqlServer
{
    using System.Collections.Generic;
    using System.Data;

    internal static class DbTypeLookup
    {
        private static readonly Dictionary<string, SqlDbType> SqlDbTypeLookup = new Dictionary<string, SqlDbType>
                                                                                    {
                                                                                        {"text", SqlDbType.Text},
                                                                                        {"uniqueidentifier", SqlDbType.UniqueIdentifier},
                                                                                        {"date", SqlDbType.Date},
                                                                                        {"time", SqlDbType.Time},
#if(!MONO)
                                                                                        {"datetime2", SqlDbType.DateTime2},
                                                                                        {
                                                                                            "datetimeoffset",
                                                                                            SqlDbType.DateTimeOffset
                                                                                        },
#endif
                                                                                        {"tinyint", SqlDbType.TinyInt},
                                                                                        {"smallint", SqlDbType.SmallInt},
                                                                                        {"int", SqlDbType.Int},
                                                                                        {"smalldatetime", SqlDbType.SmallDateTime},
                                                                                        {"real", SqlDbType.Real},
                                                                                        {"money", SqlDbType.Money},
                                                                                        {"datetime", SqlDbType.DateTime},
                                                                                        {"float", SqlDbType.Float},
                                                                                        {"sql_variant", SqlDbType.Variant},
                                                                                        {"ntext", SqlDbType.NText},
                                                                                        {"bit", SqlDbType.Bit},
                                                                                        {"decimal", SqlDbType.Decimal},
                                                                                        {"numeric", SqlDbType.Decimal},
                                                                                        {"smallmoney", SqlDbType.SmallMoney},
                                                                                        {"bigint", SqlDbType.BigInt},
                                                                                        {"varbinary", SqlDbType.VarBinary},
                                                                                        {"varchar", SqlDbType.VarChar},
                                                                                        {"binary", SqlDbType.Binary},
                                                                                        {"char", SqlDbType.Char},
                                                                                        {"timestamp", SqlDbType.Binary},
                                                                                        {"nvarchar", SqlDbType.NVarChar},
                                                                                        {"nchar", SqlDbType.NChar},
                                                                                        {"xml", SqlDbType.Xml},
                                                                                        {"image", SqlDbType.Image},
                                                                                    };

        public static SqlDbType GetSqlDbType(string typeName)
        {
            return SqlDbTypeLookup[typeName];
        }
    }
}