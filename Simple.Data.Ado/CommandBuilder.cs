using System;
using System.Collections;
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

        public ParameterTemplate AddParameter(string name, DbType dbType, object value)
        {
            name = _schemaProvider.NameParameter(name);
            var parameterTemplate = new ParameterTemplate(name, dbType, 0);
            _parameters.Add(parameterTemplate, value);
            return parameterTemplate;
        }

        public void Append(string text)
        {
            _text.Append(text);
        }

        public void SetText(string text)
        {
            _text.Clear();
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

        public CommandTemplate GetCommandTemplate(Table table)
        {
            var index = table.Columns.Select((c, i) => Tuple.Create(c, i)).ToDictionary(t => t.Item1.ActualName, t => t.Item2);
            return new CommandTemplate(_text.ToString(), _parameters.Keys.ToArray(), new Dictionary<string, int>(index, HomogenizedEqualityComparer.DefaultInstance));
        }

        private void SetParameters(IDbCommand command)
        {
            if (_parameters.Any(kvp => kvp.Value is IRange) || _parameters.Any(kvp => kvp.Value is IEnumerable && !(kvp.Value is string)))
            {
                foreach (var pair in _parameters)
                {
                    foreach (var parameter in CreateParameterComplex(pair.Key, pair.Value, command))
                    {
                        command.Parameters.Add(parameter);
                    }
                }
            }
            else
            {
                foreach (var pair in _parameters)
                {
                    command.Parameters.Add(CreateSingleParameter(pair.Value, command, pair.Key.Name, pair.Key.DbType));
                }
            }
        }

        private static IEnumerable<IDbDataParameter> CreateParameterComplex(ParameterTemplate template, object value, IDbCommand command)
        {
            var str = value as string;
            if (str != null)
            {
                yield return CreateSingleParameter(value, command, template.Name, template.DbType);
            }
            else
            {
                var range = value as IRange;
                if (range != null)
                {
                    yield return CreateSingleParameter(range.Start, command, template.Name + "_start", template.DbType);
                    yield return CreateSingleParameter(range.End, command, template.Name + "_end", template.DbType);
                    SetBetweenInCommandText(command, template.Name);
                }
                else
                {
                    var list = value as IEnumerable;
                    if (list != null)
                    {
                        var builder = new StringBuilder();
                        var array = list.Cast<object>().ToArray();
                        for (int i = 0; i < array.Length; i++)
                        {
                            builder.AppendFormat(",{0}_{1}", template.Name, i);
                            yield return
                                CreateSingleParameter(array[i], command, template.Name + "_" + i, template.DbType);
                        }
                        if (command.CommandText.Contains("!= " + template.Name))
                        {
                            command.CommandText = command.CommandText.Replace("!= " + template.Name,
                                                                              "NOT IN (" +
                                                                              builder.ToString().Substring(1) + ")");
                        }
                        else
                        {
                            command.CommandText = command.CommandText.Replace("= " + template.Name,
                                                                              "IN (" + builder.ToString().Substring(1) +
                                                                              ")");
                        }
                    }
                    else
                    {
                        yield return CreateSingleParameter(value, command, template.Name, template.DbType);
                    }
                }
            }
        }

        public static void SetBetweenInCommandText(IDbCommand command, string name)
        {
            if (command.CommandText.Contains("!= " + name))
            {
                command.CommandText = command.CommandText.Replace("!= " + name,
                                                                  string.Format("NOT BETWEEN {0}_start AND {0}_end", name));
            }
            else
            {
                command.CommandText = command.CommandText.Replace("= " + name,
                                                                  string.Format("BETWEEN {0}_start AND {0}_end", name));
            }
        }

        private static IDbDataParameter CreateSingleParameter(object value, IDbCommand command, string name, DbType dbType)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = dbType;
            parameter.Value = GetTheDataParameterValue(value);
            return parameter;
        }

        private static object GetTheDataParameterValue(object value)
        {
            return value ?? DBNull.Value;
        }
    }
}
