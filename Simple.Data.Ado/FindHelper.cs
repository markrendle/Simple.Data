using System;
using System.Collections.Generic;
using System.Text;
using Shitty.Data.Ado.Schema;

namespace Shitty.Data.Ado
{
    using System.Linq;

    internal class FindHelper
    {
        private readonly DatabaseSchema _schema;
        private readonly ICommandBuilder _commandBuilder;
        private readonly IExpressionFormatter _expressionFormatter;

        public FindHelper(DatabaseSchema schema)
        {
            _schema = schema;
            _commandBuilder = new CommandBuilder(schema);
            _expressionFormatter = new ExpressionFormatter(_commandBuilder, _schema);
        }

        public ICommandBuilder GetFindByCommand(ObjectName tableName, SimpleExpression criteria)
        {
            _commandBuilder.Append(GetSelectClause(tableName));

            if (criteria != null)
            {
                _commandBuilder.Append(" ");
                _commandBuilder.Append(string.Join(" ", new Joiner(JoinType.Inner, _schema).GetJoinClauses(tableName, criteria)));
                _commandBuilder.Append(" where ");
                _commandBuilder.Append(_expressionFormatter.Format(criteria));
            }

            return _commandBuilder;
        }

        private string GetSelectClause(ObjectName tableName)
        {
            var table = _schema.FindTable(tableName);
            return string.Format("select {0} from {1}", string.Join(", ", table.Columns.Select(c => c.QualifiedName)), table.QualifiedName);
        }
    }

    internal class GetHelper
    {
        private readonly DatabaseSchema _schema;
        private readonly ICommandBuilder _commandBuilder;

        public GetHelper(DatabaseSchema schema)
        {
            _schema = schema;
            _commandBuilder = new CommandBuilder(schema);
        }

        public ICommandBuilder GetCommand(Table table, params object[] keyValues)
        {
            _commandBuilder.Append(GetSelectClause(table));
            var param = _commandBuilder.AddParameter(keyValues[0], table.FindColumn(table.PrimaryKey[0]));
            _commandBuilder.Append(string.Format(" where {0} = {1}", _schema.QuoteObjectName(table.PrimaryKey[0]), param.Name));
            for (int i = 1; i < table.PrimaryKey.Length; i++)
            {
                param = _commandBuilder.AddParameter(keyValues[i], table.FindColumn(table.PrimaryKey[i]));
                _commandBuilder.Append(string.Format(" and {0} = {1}", _schema.QuoteObjectName(table.PrimaryKey[i]), param.Name));
            }

            return _commandBuilder;
        }

        private string GetSelectClause(Table table)
        {
            return string.Format("select {0} from {1}", string.Join(", ", table.Columns.Select(c => c.QualifiedName)), table.QualifiedName);
        }
    }
}
