using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    using System.Data;

    public static class DbConnectionEx
    {
        public static void OpenIfClosed(this IDbConnection connection)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
        }

        public static IDbCommand CreateCommand(this IDbConnection connection, AdoOptions options)
        {
            if (options == null || options.CommandTimeout < 0) return connection.CreateCommand();

            var command = connection.CreateCommand();
            command.CommandTimeout = options.CommandTimeout;
            return command;
        }
    }
}
