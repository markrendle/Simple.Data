using System.Linq;
using System.Data;
using System.Text.RegularExpressions;

namespace Simple.Data.Mocking.Ado
{
    public static class MockDatabase
    {
        public static void Record(IDbCommand command)
        {
            Sql = Regex.Replace(command.CommandText, @"\s+", " ");
            Parameters = command.Parameters.Cast<IDataParameter>().Select(p => p.Value).ToArray();
        }

        public static string Sql { get; private set; }
        public static object[] Parameters { get; private set; }
    }
}
