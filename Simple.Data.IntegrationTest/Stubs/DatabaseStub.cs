using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Simple.Data.IntegrationTest.Stubs
{
    public static class DatabaseStub
    {
        public static void Record(IDbCommand command)
        {
            Sql = command.CommandText;
            Parameters = command.Parameters.Cast<IDataParameter>().Select(p => p.Value).ToArray();
        }

        public static string Sql { get; private set; }
        public static object[] Parameters { get; private set; }
    }
}
