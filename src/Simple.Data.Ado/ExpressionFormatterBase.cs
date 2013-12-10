using System;
using System.Collections;
using System.Collections.Generic;

namespace Simple.Data.Ado
{
    using Schema;

    abstract class ExpressionFormatterBase : IExpressionFormatter
    {
        private readonly Lazy<Operators> _operators; 
        private readonly Dictionary<SimpleExpressionType, Func<SimpleExpression, string>> _expressionFormatters;

        protected ExpressionFormatterBase(Func<Operators> createOperators)
        {
            _operators = new Lazy<Operators>(createOperators);
            _expressionFormatters = new Dictionary<SimpleExpressionType, Func<SimpleExpression, string>>
                                        {
                                            {SimpleExpressionType.And, LogicalExpressionToWhereClause},
                                            {SimpleExpressionType.Or, LogicalExpressionToWhereClause},
                                            {SimpleExpressionType.Equal, EqualExpressionToWhereClause},
                                            {SimpleExpressionType.NotEqual, NotEqualExpressionToWhereClause},
                                            {SimpleExpressionType.Function, FunctionExpressionToWhereClause},
                                            {SimpleExpressionType.GreaterThan, expr => BinaryExpressionToWhereClause(expr, Operators.GreaterThan)},
                                            {SimpleExpressionType.GreaterThanOrEqual, expr => BinaryExpressionToWhereClause(expr, Operators.GreaterThanOrEqual)},
                                            {SimpleExpressionType.LessThan, expr => BinaryExpressionToWhereClause(expr, Operators.LessThan)},
                                            {SimpleExpressionType.LessThanOrEqual, expr => BinaryExpressionToWhereClause(expr, Operators.LessThanOrEqual)},
                                            {SimpleExpressionType.Empty, expr => string.Empty },
                                        };
        }

        protected Operators Operators
        {
            get { return _operators.Value; }
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
            if (expression.RightOperand == null) return string.Format("{0} {1}", FormatObject(expression.LeftOperand, null), Operators.IsNull);
            if (CommonTypes.Contains(expression.RightOperand.GetType())) return FormatAsComparison(expression, Operators.Equal);

            return FormatAsComparison(expression, Operators.Equal);
        }

        private string NotEqualExpressionToWhereClause(SimpleExpression expression)
        {
            if (expression.RightOperand == null) return string.Format("{0} {1}", FormatObject(expression.LeftOperand, null), Operators.IsNotNull);
            if (CommonTypes.Contains(expression.RightOperand.GetType())) return FormatAsComparison(expression, Operators.NotEqual);

            return FormatAsComparison(expression, Operators.NotEqual);
        }

        private string FormatAsComparison(SimpleExpression expression, string op)
        {
            return string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand, expression.RightOperand), op,
                                 FormatObject(expression.RightOperand, expression.LeftOperand));
        }

        private string FunctionExpressionToWhereClause(SimpleExpression expression)
        {
            var function = expression.RightOperand as SimpleFunction;
            if (function == null) throw new InvalidOperationException("Expected SimpleFunction as the right operand.");

            if (function.Name.Equals("like", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand, expression.RightOperand), Operators.Like,
                                     FormatObject(function.Args[0], expression.LeftOperand));
            }

            if (function.Name.Equals("notlike", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Format("{0} {1} {2}", FormatObject(expression.LeftOperand, expression.RightOperand), Operators.NotLike,
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

    public class Operators
    {
        public virtual string Equal { get { return "="; } }
        public virtual string NotEqual { get { return "!="; } }
        public virtual string GreaterThan { get { return ">"; } }
        public virtual string GreaterThanOrEqual { get { return ">="; } }
        public virtual string LessThan { get { return "<"; } }
        public virtual string LessThanOrEqual { get { return "<="; } }
        public virtual string IsNull { get { return "IS NULL"; } }
        public virtual string IsNotNull { get { return "IS NOT NULL"; } }
        public virtual string Like { get { return "LIKE"; } }
        public virtual string NotLike { get { return "NOT LIKE"; } }
        public virtual string In { get { return "IN"; } }
        public virtual string NotIn { get { return "NOT IN"; } }
        public virtual string Between { get { return "BETWEEN"; } }
        public virtual string NotBetween { get { return "NOT BETWEEN"; } }
        public virtual string Add { get { return "+"; } }
        public virtual string Subtract { get { return "-"; } }
        public virtual string Multiply { get { return "*"; } }
        public virtual string Divide { get { return "/"; } }
        public virtual string Modulo { get { return "%"; } }
    }
}