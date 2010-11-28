using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    class ExpressionFormatter : IExpressionFormatter
    {
        private readonly ICommandBuilder _commandBuilder;
        private readonly DatabaseSchema _schema;
        private readonly Dictionary<SimpleExpressionType, Func<SimpleExpression, string>> _expressionFormatters;

        public ExpressionFormatter(ICommandBuilder commandBuilder, DatabaseSchema schema)
        {
            _commandBuilder = commandBuilder;
            _schema = schema;
            _expressionFormatters = new Dictionary<SimpleExpressionType, Func<SimpleExpression, string>>
                  {
                      {SimpleExpressionType.And, LogicalExpressionToWhereClause},
                      {SimpleExpressionType.Or, LogicalExpressionToWhereClause},
                      {SimpleExpressionType.Equal, EqualExpressionToWhereClause},
                      {SimpleExpressionType.NotEqual, NotEqualExpressionToWhereClause},
                      {SimpleExpressionType.GreaterThan, expr => BinaryExpressionToWhereClause(expr, ">")},
                      {SimpleExpressionType.GreaterThanOrEqual, expr => BinaryExpressionToWhereClause(expr, ">=")},
                      {SimpleExpressionType.LessThan, expr => BinaryExpressionToWhereClause(expr, "<")},
                      {SimpleExpressionType.LessThanOrEqual, expr => BinaryExpressionToWhereClause(expr, "<=")},
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

        private string NotEqualExpressionToWhereClause(SimpleExpression expression)
        {
            return string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand),
                                 expression.RightOperand is string ? "NOT LIKE" : "!=",
                                 FormatObject(expression.RightOperand));
        }

        private string BinaryExpressionToWhereClause(SimpleExpression expression, string comparisonOperator)
        {
            return string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand),
                                 comparisonOperator,
                                 FormatObject(expression.RightOperand));
        }

        private string FormatObject(object value)
        {
            var reference = value as DynamicReference;
            if (!ReferenceEquals(reference, null))
            {
                var table = _schema.FindTable(reference.GetOwner().GetName());
                return table.QualifiedName + "." + table.FindColumn(reference.GetName()).QuotedName;
            }

            return _commandBuilder.AddParameter(value);
        }
    }
}
