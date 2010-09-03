using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Simple.Data.Ado
{
    internal static class CommandHelper
    {
        internal static IDbCommand Create(IDbConnection connection, string sql, IList<object> values)
        {
            var command = connection.CreateCommand();

            command.CommandText = PrepareCommand(sql, values, command);

            return command;
        }

        private static string PrepareCommand(IEnumerable<char> sql, IList<object> values, IDbCommand command)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            foreach (var c in sql)
            {
                if (c == '?')
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@p" + index;
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
    }
}
