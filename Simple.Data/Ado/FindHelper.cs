using System;
using System.Collections.Generic;
using System.Text;
using Simple.Data.Schema;

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
            _expressionFormatter = new ExpressionFormatter(_commandBuilder);
        }

        public string GetFindBySql(string tableName, IDictionary<string, object> criteria)
        {
            var sqlBuilder = new StringBuilder("select * from " + tableName);
            var keyword = "where";

            foreach (var criterion in criteria)
            {
                sqlBuilder.AppendFormat(" {0} {1} = ?", keyword, criterion.Key);
                if (keyword == "where")
                {
                    keyword = "and";
                }
            }

            return sqlBuilder.ToString();
        }

        public ICommandBuilder GetFindByCommand(string tableName, SimpleExpression criteria)
        {
            _commandBuilder.Append("select * from " + tableName);
            _commandBuilder.Append(" ");
            _commandBuilder.Append(new Joiner(JoinType.Inner, _schema).GetJoinClauses(tableName, criteria));

            if (criteria != null)
            {
                _commandBuilder.Append(" where ");
                _commandBuilder.Append(_expressionFormatter.Format(criteria));
            }

            return _commandBuilder;
        }
    }
}
