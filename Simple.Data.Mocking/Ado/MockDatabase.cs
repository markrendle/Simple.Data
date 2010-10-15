using System.Linq;
using System.Data;

namespace Simple.Data.Mocking.Ado
{
    public static class MockDatabase
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
