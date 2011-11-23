using System;
using System.Collections.Generic;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
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
}
