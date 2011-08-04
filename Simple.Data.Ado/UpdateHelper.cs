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

            if (criteria != null )
            {
                var whereStatement = _expressionFormatter.Format(criteria);
                if (!string.IsNullOrEmpty(whereStatement))
                  _commandBuilder.Append(" where " + whereStatement);
            }

            return _commandBuilder;
        }

        private string GetUpdateClause(string tableName, IEnumerable<KeyValuePair<string, object>> data)
        {
            var table = _schema.FindTable(tableName);
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
