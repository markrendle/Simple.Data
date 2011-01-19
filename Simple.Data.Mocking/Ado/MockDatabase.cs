using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text.RegularExpressions;

namespace Simple.Data.Mocking.Ado
{
    public class MockDatabase
    {
        private readonly List<string> _commandTexts = new List<string>();
        private readonly List<Dictionary<string,object>> _commandParameters = new List<Dictionary<string, object>>();
        public void Record(IDbCommand command)
        {
            Sql = Regex.Replace(command.CommandText, @"\s+", " ");
            CommandTexts.Add(Sql);
            Parameters = command.Parameters.Cast<IDataParameter>().Select(p => p.Value).ToArray();
            _commandParameters.Add(command.Parameters.Cast<IDataParameter>().ToDictionary(p => p.ParameterName, p => p.Value));
        }

        public string Sql { get; private set; }
        public object[] Parameters { get; private set; }

        public List<string> CommandTexts
        {
            get { return _commandTexts; }
        }

        public List<Dictionary<string, object>> CommandParameters
        {
            get { return _commandParameters; }
        }
    }
}
