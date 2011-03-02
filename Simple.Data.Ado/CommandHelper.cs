using System.Collections.Generic;
using System.Data;
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
                    parameter.Value = values[index];
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
                parameter.ParameterName = pair.Key;
                parameter.Value = pair.Value;
                command.Parameters.Add(parameter);
            }
        }
    }
}
