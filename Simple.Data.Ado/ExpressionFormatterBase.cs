using System;
using System.Collections;
using System.Collections.Generic;

namespace Simple.Data.Ado
{
    abstract class ExpressionFormatterBase : IExpressionFormatter
    {
        private readonly Dictionary<SimpleExpressionType, Func<SimpleExpression, string>> _expressionFormatters;

        protected ExpressionFormatterBase()
        {
            _expressionFormatters = new Dictionary<SimpleExpressionType, Func<SimpleExpression, string>>
                                        {
                                            {SimpleExpressionType.And, LogicalExpressionToWhereClause},
                                            {SimpleExpressionType.Or, LogicalExpressionToWhereClause},
                                            {SimpleExpressionType.Equal, EqualExpressionToWhereClause},
                                            {SimpleExpressionType.NotEqual, NotEqualExpressionToWhereClause},
                                            {SimpleExpressionType.Function, FunctionExpressionToWhereClause},
                                            {SimpleExpressionType.GreaterThan, expr => BinaryExpressionToWhereClause(expr, ">")},
                                            {SimpleExpressionType.GreaterThanOrEqual, expr => BinaryExpressionToWhereClause(expr, ">=")},
                                            {SimpleExpressionType.LessThan, expr => BinaryExpressionToWhereClause(expr, "<")},
                                            {SimpleExpressionType.LessThanOrEqual, expr => BinaryExpressionToWhereClause(expr, "<=")},
                                            {SimpleExpressionType.Empty, expr => string.Empty },
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
            if (expression.RightOperand == null) return string.Format("{0} IS NULL", FormatObject(expression.LeftOperand, null));
            if (CommonTypes.Contains(expression.RightOperand.GetType())) return FormatAsComparison(expression, "=");

            return FormatAsComparison(expression, "=");
        }

        private string NotEqualExpressionToWhereClause(SimpleExpression expression)
        {
            if (expression.RightOperand == null) return string.Format("{0} IS NOT NULL", FormatObject(expression.LeftOperand, null));
            if (CommonTypes.Contains(expression.RightOperand.GetType())) return FormatAsComparison(expression, "!=");

            return FormatAsComparison(expression, "!=");
        }

        private string FormatAsComparison(SimpleExpression expression, string op)
        {
            return string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand, expression.RightOperand), op,
                                 FormatObject(expression.RightOperand, expression.LeftOperand));
        }

        private string TryFormatAsInList(SimpleExpression expression, IEnumerable list, string op)
        {
            return (list != null)
                       ?
                           string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand, expression.RightOperand), op, FormatList(list, expression.LeftOperand))
                       :
                           null;
        }

        private string TryFormatAsRange(SimpleExpression expression, IRange range, string op)
        {
            return (range != null)
                       ?
                           string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand, expression.RightOperand), op, FormatRange(range, expression.LeftOperand))
                       :
                           null;
        }

        private string FunctionExpressionToWhereClause(SimpleExpression expression)
        {
            var function = expression.RightOperand as SimpleFunction;
            if (function == null) throw new InvalidOperationException("Expected SimpleFunction as the right operand.");

            if (function.Name.Equals("like", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Format("{0} LIKE {1}", FormatObject(expression.LeftOperand, expression.RightOperand),
                                     FormatObject(function.Args[0], expression.LeftOperand));
            }

            if (function.Name.Equals("notlike", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Format("{0} NOT LIKE {1}", FormatObject(expression.LeftOperand, expression.RightOperand),
                                     FormatObject(function.Args[0], expression.LeftOperand));
            }

            throw new NotSupportedException(string.Format("Unknown function '{0}'.", function.Name));
        }

        private string BinaryExpressionToWhereClause(SimpleExpression expression, string comparisonOperator)
        {
            return string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand, expression.RightOperand),
                                 comparisonOperator,
                                 FormatObject(expression.RightOperand, expression.LeftOperand));
        }

        protected abstract string FormatObject(object value, object otherOperand);
        protected abstract string FormatRange(IRange range, object otherOperand);
        protected abstract string FormatList(IEnumerable list, object otherOperand);
    }
}