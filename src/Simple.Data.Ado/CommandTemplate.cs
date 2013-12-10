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
        private readonly Func<IDbCommand, IDbParameterFactory> _createGetParameterFactoryFunc;
        private readonly string _commandText;
        private readonly ParameterTemplate[] _parameters;
        private readonly Dictionary<string, int> _index;

        public CommandTemplate(Func<IDbCommand, IDbParameterFactory> createGetParameterFactoryFunc, string commandText, ParameterTemplate[] parameterNames, Dictionary<string, int> index)
        {
            _createGetParameterFactoryFunc = createGetParameterFactoryFunc;
            _commandText = commandText;
            _parameters = parameterNames;
            _index = index;
        }

        public Dictionary<string, int> Index
        {
            get { return _index; }
        }

        public IDbCommand GetDbCommand(AdoAdapter adapter, IDbConnection connection, IEnumerable<object> parameterValues)
        {
            var command = connection.CreateCommand(adapter.AdoOptions);
            command.CommandText = _commandText;

            foreach (var parameter in CreateParameters(adapter.GetSchema(), command, parameterValues))
            {
                command.Parameters.Add(parameter);
            }
            return command;
        }

        private IEnumerable<IDbDataParameter> CreateParameters(DatabaseSchema schema, IDbCommand command, IEnumerable<object> parameterValues)
        {
            var fixedParameters = _parameters.Where(pt => pt.Type == ParameterType.FixedValue).ToArray();
            if ((!parameterValues.Any(pv => pv != null)) && fixedParameters.Length == 0) yield break;
            parameterValues = parameterValues.Where(pv => pv != null);

            foreach (var fixedParameter in fixedParameters)
            {
                yield return CreateParameter(command, fixedParameter, fixedParameter.FixedValue);
            }
            
            var columnParameters = _parameters.Where(pt => pt.Type != ParameterType.FixedValue).ToArray();

            foreach (var parameter in parameterValues.Any(o => o is IEnumerable && !(o is string)) || parameterValues.Any(o => o is IRange)
                       ? parameterValues.SelectMany((v, i) => CreateParameters(schema, command, columnParameters[i], v))
                       : parameterValues.Select((v, i) => CreateParameter(command, columnParameters[i], v)))
            {
                yield return parameter;
            }
        }

        private IEnumerable<IDbDataParameter> CreateParameters(DatabaseSchema schema, IDbCommand command, ParameterTemplate parameterTemplate, object value)
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
                    new CommandBuilder(schema).SetBetweenInCommandText(command, parameterTemplate.Name);
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
                        RewriteSqlEqualityToInClause(schema, command, parameterTemplate, builder);
                    }
                    else
                    {
                        yield return CreateParameter(command, parameterTemplate, value);
                    }
                }
            }
        }

        private static void RewriteSqlEqualityToInClause(DatabaseSchema schema, IDbCommand command, ParameterTemplate parameterTemplate, StringBuilder builder)
        {
            if (command.CommandText.Contains("!= " + parameterTemplate.Name))
            {
                command.CommandText = command.CommandText.Replace("!= " + parameterTemplate.Name,
                                                                  schema.Operators.NotIn + " (" +
                                                                  builder.ToString().Substring(1) +
                                                                  ")");
            }
            else
            {
                command.CommandText = command.CommandText.Replace("= " + parameterTemplate.Name,
                                                                  schema.Operators.In + " (" +
                                                                  builder.ToString().Substring(1) +
                                                                  ")");
            }
        }

        private IDbDataParameter CreateParameter(IDbCommand command, ParameterTemplate parameterTemplate, object value, string suffix = "")
        {
            var factory = _createGetParameterFactoryFunc(command);
            var parameter = default(IDbDataParameter);
            if(parameterTemplate.Column != null)
            {
                parameter = factory.CreateParameter(parameterTemplate.Name + suffix,
                                                    parameterTemplate.Column);
            }
            else if (parameterTemplate.Type == ParameterType.NameOnly)
            {
                parameter = factory.CreateParameter(parameterTemplate.Name);
            }
            else
            {
                parameter = factory.CreateParameter(parameterTemplate.Name, parameterTemplate.DbType,
                                                    parameterTemplate.MaxLength);
            }
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