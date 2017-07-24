using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shitty.Data.Ado.Schema;

namespace Shitty.Data.Ado
{
    internal class DeleteHelper
    {
        private readonly DatabaseSchema _schema;
        private readonly ICommandBuilder _commandBuilder;
        private readonly IExpressionFormatter _expressionFormatter;

        public DeleteHelper(DatabaseSchema schema)
        {
            _schema = schema;
            _commandBuilder = new CommandBuilder(schema);
            _expressionFormatter = new ExpressionFormatter(_commandBuilder, _schema);
        }

        public ICommandBuilder GetDeleteCommand(string tableName, SimpleExpression criteria)
        {
            _commandBuilder.Append(GetDeleteClause(tableName));

            if (criteria != null)
            {
                var whereCondition = _expressionFormatter.Format(criteria);
                if (!string.IsNullOrEmpty(whereCondition))
                    _commandBuilder.Append(" where " + whereCondition);
            }

            return _commandBuilder;
        }

        private string GetDeleteClause(string tableName)
        {
            var table = _schema.FindTable(tableName);
            return string.Concat("delete from ", table.QualifiedName);
        }
    }
}
