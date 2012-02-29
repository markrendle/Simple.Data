using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    using System.Diagnostics;
    using Extensions;

    internal class UpdateHelper
    {
        private readonly DatabaseSchema _schema;
        private readonly ICommandBuilder _commandBuilder;
        private readonly IExpressionFormatter _expressionFormatter;

        public UpdateHelper(DatabaseSchema schema)
        {
            _schema = schema;
            _commandBuilder = new CommandBuilder(schema);
            _expressionFormatter = new ExpressionFormatter(_commandBuilder, _schema);
        }

        public ICommandBuilder GetUpdateCommand(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            var table = _schema.FindTable(tableName);
            _commandBuilder.Append(GetUpdateClause(table, data));

            if (criteria != null )
            {
                string whereStatement = null;
                if (criteria.GetOperandsOfType<ObjectReference>().Any(o => !o.GetOwner().GetName().Equals(tableName)))
                {
                    if (table.PrimaryKey.Length == 1)
                    {
                        whereStatement = CreateWhereInStatement(criteria, table);
                    }
                }
                else
                {
                    whereStatement = _expressionFormatter.Format(criteria);
                }
                if (!string.IsNullOrEmpty(whereStatement))
                  _commandBuilder.Append(" where " + whereStatement);
            }

            return _commandBuilder;
        }

        private string CreateWhereInStatement(SimpleExpression criteria, Table table)
        {
            var inClauseBuilder = new CommandBuilder(_schema);
            var keyColumn = table.FindColumn(table.PrimaryKey[0]);
            inClauseBuilder.Append(string.Format("SELECT {0} FROM {1}",
                                                 keyColumn.QualifiedName, table.QualifiedName));
            inClauseBuilder.Append(" ");
            inClauseBuilder.Append(string.Join(" ",
                                               new Joiner(JoinType.Inner, _schema).GetJoinClauses(
                                                   new ObjectName(table.Schema, table.ActualName), criteria)));
            inClauseBuilder.Append(" where ");
            inClauseBuilder.Append(_expressionFormatter.Format(criteria));
            return string.Format("{0} IN ({1})", keyColumn.QualifiedName, inClauseBuilder.Text);
        }

        private string GetUpdateClause(Table table, IEnumerable<KeyValuePair<string, object>> data)
        {
            var setClause = string.Join(", ",
                data.Where(kvp => table.HasColumn(kvp.Key))
                .Select(kvp => CreateColumnUpdateClause(kvp.Key, kvp.Value, table)));
            return string.Format("update {0} set {1}", table.QualifiedName, setClause);
        }

        private string CreateColumnUpdateClause(string columnName, object value, Table table)
        {
            var column = table.FindColumn(columnName);
            return string.Format("{0} = {1}", column.QuotedName, _commandBuilder.AddParameter(value, column).Name);
        }
    }
}
