using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    internal class CommandHelper
    {
        private readonly ISchemaProvider _schemaProvider;

        public CommandHelper(ISchemaProvider schemaProvider)
        {
            _schemaProvider = schemaProvider;
        }

        internal IDbCommand Create(IDbConnection connection, string sql, IList<object> values)
        {
            var command = connection.CreateCommand();

            command.CommandText = PrepareCommand(sql, values, command);

            return command;
        }

        internal IDbCommand Create(IDbConnection connection, CommandBuilder commandBuilder)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandBuilder.Text;
            PrepareCommand(commandBuilder, command);
            return command;
        }

        private string PrepareCommand(IEnumerable<char> sql, IList<object> values, IDbCommand command)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            foreach (var c in sql)
            {
                if (c == '?')
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = _schemaProvider.NameParameter("p" + index);
                    parameter.Value = FixObjectType(values[index]);
                    command.Parameters.Add(parameter);
                    
                    sqlBuilder.Append(parameter.ParameterName);
                    index++;
                }
                else
                {
                    sqlBuilder.Append(c);
                }
            }
            return sqlBuilder.ToString();
        }

        private static void PrepareCommand(CommandBuilder commandBuilder, IDbCommand command)
        {
            foreach (var pair in commandBuilder.Parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = pair.Key.Name;
                parameter.DbType = pair.Key.DbType;
                parameter.Size = pair.Key.MaxLength;
                parameter.Value = FixObjectType(pair.Value);
                command.Parameters.Add(parameter);
            }
        }

        public static object FixObjectType(object value)
        {
            if (value == null) return DBNull.Value;
            if (TypeHelper.IsKnownType(value.GetType())) return value;
            var asString = value.ToString();
            if (asString != value.GetType().FullName) return asString;
            return value;
        }
    }

    class ConvertibleValue : IConvertible
    {
        private readonly dynamic _value;

        public ConvertibleValue(dynamic value)
        {
            _value = value;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return _value;
        }

        public char ToChar(IFormatProvider provider)
        {
            return _value;
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return _value;
        }

        public byte ToByte(IFormatProvider provider)
        {
            return _value;
        }

        public short ToInt16(IFormatProvider provider)
        {
            return _value;
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return _value;
        }

        public int ToInt32(IFormatProvider provider)
        {
            return _value;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return _value;
        }

        public long ToInt64(IFormatProvider provider)
        {
            return _value;
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return _value;
        }

        public float ToSingle(IFormatProvider provider)
        {
            return _value;
        }

        public double ToDouble(IFormatProvider provider)
        {
            return _value;
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return _value;
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return _value;
        }

        public string ToString(IFormatProvider provider)
        {
            return _value;
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return Convert.ChangeType(_value, conversionType);
        }
    }
}
