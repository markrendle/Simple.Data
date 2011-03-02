using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    internal class DeleteHelper
    {
        private readonly DatabaseSchema _schema;
        private readonly ICommandBuilder _commandBuilder;
        private readonly IExpressionFormatter _expressionFormatter;

        public DeleteHelper(DatabaseSchema schema)
        {
            _schema = schema;
            _commandBuilder = new CommandBuilder(schema.SchemaProvider);
            _expressionFormatter = new ExpressionFormatter(_commandBuilder, _schema);
        }

        public ICommandBuilder GetDeleteCommand(string tableName, SimpleExpression criteria)
        {
            _commandBuilder.Append(GetDeleteClause(tableName));

            if (criteria != null)
            {
                _commandBuilder.Append(" where ");
                _commandBuilder.Append(_expressionFormatter.Format(criteria));
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
