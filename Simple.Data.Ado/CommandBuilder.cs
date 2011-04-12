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
        private readonly Dictionary<ParameterTemplate, object> _parameters = new Dictionary<ParameterTemplate, object>();
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

        public ParameterTemplate AddParameter(object value, Column column)
        {
            string name = _schemaProvider.NameParameter("p" + Interlocked.Increment(ref _number));
            var parameterTemplate = column == null
                                        ? new ParameterTemplate(name)
                                        : new ParameterTemplate(name, column.DbType, column.MaxLength);
            _parameters.Add(parameterTemplate, value);
            return parameterTemplate;
        }

        public void Append(string text)
        {
            _text.Append(text);
        }

        public override string ToString()
        {
            return _text.ToString();
        }

        public IEnumerable<KeyValuePair<ParameterTemplate, object>> Parameters
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
                parameter.ParameterName = pair.Key.Name;
                parameter.DbType = pair.Key.DbType;
                parameter.Value = pair.Value;
                command.Parameters.Add(parameter);
            }
        }
    }

    public class CommandTemplate
    {
        private readonly string _commandText;
        private readonly ParameterTemplate[] _parameters;

        public CommandTemplate(string commandText, ParameterTemplate[] parameterNames)
        {
            _commandText = commandText;
            _parameters = parameterNames;
        }

        public IDbCommand GetDbCommand(IDbConnection connection, IEnumerable<object> parameterValues)
        {
            var command = connection.CreateCommand();
            command.CommandText = _commandText;
            var parameters = parameterValues
                .Select((v, i) => CreateParameter(command, _parameters[i], v));
            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }

        private static IDbDataParameter CreateParameter(IDbCommand command, ParameterTemplate parameterTemplate, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterTemplate.Name;
            parameter.DbType = parameterTemplate.DbType;
            parameter.Value = value;
            return parameter;
        }
    }

    public class ParameterTemplate : IEquatable<ParameterTemplate>
    {
        private readonly string _name;
        private readonly DbType _dbType;
        private readonly int _maxLength;

        public ParameterTemplate(string name) : this(name, DbType.Object, 0)
        {
        }

        public ParameterTemplate(string name, DbType dbType, int maxLength)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
            _dbType = dbType;
            _maxLength = maxLength;
        }

        public int MaxLength
        {
            get { return _maxLength; }
        }

        public DbType DbType
        {
            get { return _dbType; }
        }

        public string Name
        {
            get { return _name; }
        }

        public bool Equals(ParameterTemplate other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._name, _name) && Equals(other._dbType, _dbType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ParameterTemplate)) return false;
            return Equals((ParameterTemplate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_name.GetHashCode()*397) ^ _dbType.GetHashCode();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
