using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    class ExpressionFormatter : IExpressionFormatter
    {
        private readonly ICommandBuilder _commandBuilder;
        private readonly Dictionary<SimpleExpressionType, Func<SimpleExpression, string>> _expressionFormatters;

        public ExpressionFormatter(ICommandBuilder commandBuilder)
        {
            _commandBuilder = commandBuilder;
            _expressionFormatters = new Dictionary<SimpleExpressionType, Func<SimpleExpression, string>>
                  {
                      {SimpleExpressionType.And, LogicalExpressionToWhereClause},
                      {SimpleExpressionType.Or, LogicalExpressionToWhereClause},
                      {SimpleExpressionType.Equal, EqualExpressionToWhereClause},
                  };
        }

        public string Format(SimpleExpression expression)
        {
            Func<SimpleExpression, string> formatter;

            if (_expressionFormatters.TryGetValue(expression.Type, out formatter))
            {
                return formatter(expression);
            }

            return string.Empty;
        }

        private string LogicalExpressionToWhereClause(SimpleExpression expression)
        {
            return string.Format("({0} {1} {2})",
                                 Format((SimpleExpression)expression.LeftOperand),
                                 expression.Type.ToString().ToUpperInvariant(),
                                 Format((SimpleExpression)expression.RightOperand));
        }

        private string EqualExpressionToWhereClause(SimpleExpression expression)
        {
            return string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand),
                                 expression.RightOperand is string ? "LIKE" : "=",
                                 FormatObject(expression.RightOperand));
        }

        private string FormatObject(object value)
        {
            var reference = value as DynamicReference;
            if (!ReferenceEquals(reference, null))
            {
                return reference.Owner.Name + "." + reference.Name;
            }

            return _commandBuilder.AddParameter(value);
        }
    }
}
