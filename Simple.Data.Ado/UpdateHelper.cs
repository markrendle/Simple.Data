using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    internal class UpdateHelper
    {
        private readonly DatabaseSchema _schema;
        private readonly ICommandBuilder _commandBuilder;
        private readonly IExpressionFormatter _expressionFormatter;

        public UpdateHelper(DatabaseSchema schema)
        {
            _schema = schema;
            _commandBuilder = new CommandBuilder(schema.SchemaProvider);
            _expressionFormatter = new ExpressionFormatter(_commandBuilder, _schema);
        }

        public ICommandBuilder GetUpdateCommand(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            _commandBuilder.Append(GetUpdateClause(tableName, data));

            if (criteria != null)
            {
                _commandBuilder.Append(" where ");
                _commandBuilder.Append(_expressionFormatter.Format(criteria));
            }

            return _commandBuilder;
        }

        private string GetUpdateClause(string tableName, IEnumerable<KeyValuePair<string, object>> data)
        {
            var table = _schema.FindTable(tableName);
            var setClause = string.Join(", ",
                data.Select(
                    kvp =>
                    string.Format("{0} = {1}", table.FindColumn(kvp.Key).QuotedName, _commandBuilder.AddParameter(kvp.Value))));
            return string.Format("update {0} set {1}", table.QualifiedName, setClause);
        }
    }
}
