using System;
using System.Collections.Generic;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    internal class FindHelper
    {
        private readonly DatabaseSchema _schema;
        private readonly ICommandBuilder _commandBuilder;
        private readonly IExpressionFormatter _expressionFormatter;

        public FindHelper(DatabaseSchema schema)
        {
            _schema = schema;
            _commandBuilder = new CommandBuilder();
            _expressionFormatter = new ExpressionFormatter(_commandBuilder, _schema);
        }

        public ICommandBuilder GetFindByCommand(ObjectName tableName, SimpleExpression criteria)
        {
            _commandBuilder.Append(GetSelectClause(tableName));
            _commandBuilder.Append(" ");
            _commandBuilder.Append(new Joiner(JoinType.Inner, _schema).GetJoinClauses(tableName, criteria));

            if (criteria != null)
            {
                _commandBuilder.Append(" where ");
                _commandBuilder.Append(_expressionFormatter.Format(criteria));
            }

            return _commandBuilder;
        }

        private string GetSelectClause(ObjectName tableName)
        {
            return string.Format("select {0}.* from {0}", _schema.FindTable(tableName).QualifiedName);
        }
    }
}
