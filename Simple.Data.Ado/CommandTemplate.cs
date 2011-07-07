using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    public class CommandTemplate
    {
        private readonly string _commandText;
        private readonly ParameterTemplate[] _parameters;
        private readonly Dictionary<string, int> _index;

        public CommandTemplate(string commandText, ParameterTemplate[] parameterNames, Dictionary<string, int> index)
        {
            _commandText = commandText;
            _parameters = parameterNames;
            _index = index;
        }

        public Dictionary<string, int> Index
        {
            get { return _index; }
        }

        public IDbCommand GetDbCommand(IDbConnection connection, IEnumerable<object> parameterValues)
        {
            var command = connection.CreateCommand();
            command.CommandText = _commandText;

            foreach (var parameter in CreateParameters(command, parameterValues))
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }

        private IEnumerable<IDbDataParameter> CreateParameters(IDbCommand command, IEnumerable<object> parameterValues)
        {
            if (!parameterValues.Any(pv => pv != null)) return Enumerable.Empty<IDbDataParameter>();

            return parameterValues.Any(o => o is IEnumerable && !(o is string)) || parameterValues.Any(o => o is IRange)
                       ? parameterValues.SelectMany((v,i) => CreateParameters(command, _parameters[i], v))
                       : parameterValues.Select((v, i) => CreateParameter(command, _parameters[i], v));
        }

        private static IEnumerable<IDbDataParameter> CreateParameters(IDbCommand command, ParameterTemplate parameterTemplate, object value)
        {
            if (value == null || TypeHelper.IsKnownType(value.GetType()) || parameterTemplate.DbType == DbType.Binary)
            {
                yield return CreateParameter(command, parameterTemplate, value);
            }
            else
            {
                var range = value as IRange;
                if (range != null)
                {
                    yield return CreateParameter(command, parameterTemplate, range.Start, "_start");
                    yield return CreateParameter(command, parameterTemplate, range.End, "_end");
                    CommandBuilder.SetBetweenInCommandText(command, parameterTemplate.Name);
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
                            builder.AppendFormat(",{0}_{1}", parameterTemplate.Name, i);
                            yield return CreateParameter(command, parameterTemplate, array[i], "_" + i);
                        }
                        RewriteSqlEqualityToInClause(command, parameterTemplate, builder);
                    }
                    else
                    {
                        yield return CreateParameter(command, parameterTemplate, value);
                    }
                }
            }
        }

        private static void RewriteSqlEqualityToInClause(IDbCommand command, ParameterTemplate parameterTemplate, StringBuilder builder)
        {
            if (command.CommandText.Contains("!= " + parameterTemplate.Name))
            {
                command.CommandText = command.CommandText.Replace("!= " + parameterTemplate.Name,
                                                                  "NOT IN (" +
                                                                  builder.ToString().Substring(1) +
                                                                  ")");
            }
            else
            {
                command.CommandText = command.CommandText.Replace("= " + parameterTemplate.Name,
                                                                  "IN (" +
                                                                  builder.ToString().Substring(1) +
                                                                  ")");
            }
        }

        private static IDbDataParameter CreateParameter(IDbCommand command, ParameterTemplate parameterTemplate, object value, string suffix = "")
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterTemplate.Name + suffix;
            parameter.DbType = parameterTemplate.DbType;
            parameter.Value = FixObjectType(value);
            return parameter;
        }

        private static object FixObjectType(object value)
        {
            if (value == null) return DBNull.Value;
            if (TypeHelper.IsKnownType(value.GetType())) return value;
            var dynamicObject = value as DynamicObject;
            if (dynamicObject != null)
            {
                return dynamicObject.ToString();
            }
            return value;
        }
    }
}