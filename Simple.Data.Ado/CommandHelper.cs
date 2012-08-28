using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    using System.Linq;

    public class CommandHelper
    {
        private readonly AdoAdapter _adapter;
        private readonly ISchemaProvider _schemaProvider;

        public CommandHelper(AdoAdapter adapter)
        {
            _adapter = adapter;
            _schemaProvider = adapter.SchemaProvider;
        }

        internal IDbCommand Create(IDbConnection connection, string sql, IList<object> values)
        {
            var command = connection.CreateCommand(_adapter.AdoOptions);

            command.CommandText = PrepareCommand(sql, command);
            command.ClearParameterValues();
            command.SetParameterValues(values);

            return command;
        }

        internal IDbCommand Create(IDbConnection connection, CommandBuilder commandBuilder)
        {
            var command = connection.CreateCommand(_adapter.AdoOptions);
            command.CommandText = commandBuilder.Text;
            PrepareCommand(commandBuilder, command);
            return command;
        }

        private string PrepareCommand(IEnumerable<char> sql, IDbCommand command)
        {
            var parameterFactory = _adapter.ProviderHelper.GetCustomProvider<IDbParameterFactory>(_schemaProvider)
                                   ?? new GenericDbParameterFactory(command);
            int index = 0;
            var sqlBuilder = new StringBuilder();
            foreach (var c in sql)
            {
                if (c == '?')
                {
                    var parameter = parameterFactory.CreateParameter(_schemaProvider.NameParameter("p" + index));
                    //parameter.ParameterName = _schemaProvider.NameParameter("p" + index);
                    command.Parameters.Add(parameter);
                    
                    sqlBuilder.Append(parameter.ParameterName);
                    index++;
                }
                else
                {
                    sqlBuilder.Append(c);
                }
            }
            return sqlBuilder.ToString();
        }

        private string PrepareInsertCommand(string sql, IDbCommand command, IEnumerable<Column> columns)
        {
            var parameterFactory = _adapter.ProviderHelper.GetCustomProvider<IDbParameterFactory>(_schemaProvider)
                                   ?? new GenericDbParameterFactory(command);

            var columnLookup = columns.ToDictionary(c => c.QuotedName, c => c);
            if (columnLookup.Count == 0) return PrepareCommand(sql, command);

            int openParenIndex = sql.IndexOf('(');
            int closeParenLength = sql.IndexOf(')') - openParenIndex;
            var columnNameList = sql.Substring(openParenIndex, closeParenLength).Trim('(', ')').Split(',');
            int index = 0;
            var sqlBuilder = new StringBuilder();
            foreach (var c in sql)
            {
                if (c == '?')
                {
                    var column = columnLookup[columnNameList[index]];
                    var parameter = parameterFactory.CreateParameter(_schemaProvider.NameParameter("p" + index), column);
                    command.Parameters.Add(parameter);
                    
                    sqlBuilder.Append(parameter.ParameterName);
                    index++;
                }
                else
                {
                    sqlBuilder.Append(c);
                }
            }
            return sqlBuilder.ToString();
        }

        private static void PrepareCommand(CommandBuilder commandBuilder, IDbCommand command)
        {
            foreach (var pair in commandBuilder.Parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = pair.Key.Name;
                parameter.DbType = pair.Key.DbType;
                parameter.Size = pair.Key.MaxLength;
                parameter.Value = FixObjectType(pair.Value);
                command.Parameters.Add(parameter);
            }
        }

        public static object FixObjectType(object value)
        {
            if (value == null) return DBNull.Value;
            if (TypeHelper.IsKnownType(value.GetType())) return value;
            if (value is Enum) return value;
            var asString = value.ToString();
            if (asString != value.GetType().FullName) return asString;
            return value;
        }

        public IDbCommand Create(IDbConnection connection, string insertSql)
        {
            var command = connection.CreateCommand(_adapter.AdoOptions);
            command.CommandText = PrepareCommand(insertSql, command);
            return command;

        }

        public IDbCommand CreateInsert(IDbConnection connection, string insertSql, IEnumerable<Column> columns)
        {
            var command = connection.CreateCommand(_adapter.AdoOptions);
            command.CommandText = PrepareInsertCommand(insertSql, command, columns);
            command.ClearParameterValues();
            return command;

        }

        public IDbCommand CreateInsert(IDbConnection connection, string sql, IEnumerable<Column> columns, IList<object> values)
        {
            var command = connection.CreateCommand(_adapter.AdoOptions);

            command.CommandText = PrepareInsertCommand(sql, command, columns);
            command.ClearParameterValues();
            command.SetParameterValues(values);

            return command;
        }       
    }
}
