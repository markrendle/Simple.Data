using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    class CommandBuilder : ICommandBuilder
    {
        private int _number;
        private readonly ISchemaProvider _schemaProvider;
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private readonly StringBuilder _text;

        public CommandBuilder(ISchemaProvider schemaProvider)
        {
            _text = new StringBuilder();
            _schemaProvider = schemaProvider;
        }

        public CommandBuilder(string text, ISchemaProvider schemaProvider)
        {
            _text = new StringBuilder(text);
            _schemaProvider = schemaProvider;
        }

        public string AddParameter(object value)
        {
            string name = _schemaProvider.NameParameter("p" + Interlocked.Increment(ref _number));
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

        public CommandTemplate GetCommandTemplate()
        {
            return new CommandTemplate(_text.ToString(), _parameters.Keys.ToArray());
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

    public class CommandTemplate
    {
        private readonly string _commandText;
        private readonly string[] _parameterNames;

        public CommandTemplate(string commandText, string[] parameterNames)
        {
            _commandText = commandText;
            _parameterNames = parameterNames;
        }

        public IDbCommand GetDbCommand(IDbConnection connection, IEnumerable<object> parameterValues)
        {
            var command = connection.CreateCommand();
            command.CommandText = _commandText;
            var parameters = parameterValues
                .Select((v, i) => CreateParameter(command, _parameterNames[i], v));
            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }

        private static IDbDataParameter CreateParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }
    }
}
