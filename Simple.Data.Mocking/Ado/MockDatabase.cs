using System.Linq;
using System.Data;
using System.Text.RegularExpressions;

namespace Simple.Data.Mocking.Ado
{
    public class MockDatabase
    {
        public void Record(IDbCommand command)
        {
            Sql = Regex.Replace(command.CommandText, @"\s+", " ");
            Parameters = command.Parameters.Cast<IDataParameter>().Select(p => p.Value).ToArray();
        }

        public string Sql { get; private set; }
        public object[] Parameters { get; private set; }
    }
}
