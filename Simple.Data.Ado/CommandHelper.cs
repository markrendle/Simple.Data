using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    using System.Linq;

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

            command.CommandText = PrepareCommand(sql, command);
            SetParameterValues(command, values);

            return command;
        }

        internal IDbCommand Create(IDbConnection connection, CommandBuilder commandBuilder)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandBuilder.Text;
            PrepareCommand(commandBuilder, command);
            return command;
        }

        private string PrepareCommand(IEnumerable<char> sql, IDbCommand command)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            foreach (var c in sql)
            {
                if (c == '?')
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = _schemaProvider.NameParameter("p" + index);
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

        public static void SetParameterValues(IDbCommand command, IList<object> values)
        {
            int index = 0;
            foreach (var parameter in command.Parameters.Cast<IDbDataParameter>())
            {
                parameter.Value = FixObjectType(values[index]);
                index++;
            }
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

        public IDbCommand Create(IDbConnection connection, string insertSql)
        {
            var command = connection.CreateCommand();
            command.CommandText = PrepareCommand(insertSql, command);
            return command;

        }
    }
}
