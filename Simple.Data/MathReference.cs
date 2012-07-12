using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public class MathReference : SimpleReference, IEquatable<MathReference>
    {
        private readonly object _leftOperand;
        private readonly object _rightOperand;
        private readonly MathOperator _operator;

        public MathReference(object leftOperand, object rightOperand, MathOperator @operator)
        {
            _leftOperand = leftOperand;
            _rightOperand = rightOperand;
            _operator = @operator;
        }

        private MathReference(object leftOperand, object rightOperand, MathOperator @operator, string alias) : base(alias)
        {
            _leftOperand = leftOperand;
            _rightOperand = rightOperand;
            _operator = @operator;
        }

        public MathOperator Operator
        {
            get { return _operator; }
        }

        public object RightOperand
        {
            get { return _rightOperand; }
        }

        public object LeftOperand
        {
            get { return _leftOperand; }
        }

        public MathReference As(string alias)
        {
            return new MathReference(_leftOperand, _rightOperand, _operator, alias);
        }

        /// <summary>
        /// Implements the operator == to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.Equal"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator ==(MathReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.Equal);
        }

        /// <summary>
        /// Implements the operator != to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.NotEqual"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator !=(MathReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.NotEqual);
        }

        /// <summary>
        /// Implements the operator &lt; to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.LessThan"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator <(MathReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThan);
        }

        /// <summary>
        /// Implements the operator &gt; to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.GreaterThan"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator >(MathReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.GreaterThan);
        }

        /// <summary>
        /// Implements the operator &lt;= to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.LessThanOrEqual"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator <=(MathReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThanOrEqual);
        }

        /// <summary>
        /// Implements the operator &gt;= to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.GreaterThanOrEqual"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator >=(MathReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.GreaterThanOrEqual);
        }

        public bool Equals(MathReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._leftOperand, _leftOperand) && Equals(other._rightOperand, _rightOperand) && Equals(other._operator, _operator);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (MathReference)) return false;
            return Equals((MathReference) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (_leftOperand != null ? _leftOperand.GetHashCode() : 0);
                result = (result*397) ^ (_rightOperand != null ? _rightOperand.GetHashCode() : 0);
                result = (result*397) ^ _operator.GetHashCode();
                return result;
            }
        }
    }

    public enum MathOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo
    }
}
