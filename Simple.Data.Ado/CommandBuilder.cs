using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;

namespace Simple.Data.Ado
{
    class CommandBuilder : ICommandBuilder
    {
        private int _number;
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private readonly StringBuilder _text;

        public CommandBuilder()
        {
            _text = new StringBuilder();
        }

        public CommandBuilder(string text)
        {
            _text = new StringBuilder(text);
        }

        public string AddParameter(object value)
        {
            string name = "@p" + Interlocked.Increment(ref _number);
            _parameters.Add(name, value);
            return name;
        }

        public void Append(string text)
        {
            _text.Append(text);
        }

        public override string ToString()
        {
            return _text.ToString();
        }

        public IEnumerable<KeyValuePair<string, object>> Parameters
        {
            get { return _parameters.AsEnumerable(); }
        }

        public string Text
        {
            get { return _text.ToString(); }
        }

        public IDbCommand GetCommand(IDbConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = Text;
            SetParameters(command);
            return command;
        }

        private void SetParameters(IDbCommand command)
        {
            foreach (var pair in _parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = pair.Key;
                parameter.Value = pair.Value;
                command.Parameters.Add(parameter);
            }
        }
    }
}
