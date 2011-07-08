using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Simple.Data
{
    /// <summary>
    /// Represents a query criteria expression.
    /// </summary>
    public class SimpleExpression
    {
        private readonly object _leftOperand;
        private readonly object _rightOperand;
        private readonly SimpleExpressionType _type;

        public SimpleExpression(object leftOperand, object rightOperand, SimpleExpressionType type)
        {
            _leftOperand = leftOperand;
            _type = type;
            _rightOperand = rightOperand;
        }

        /// <summary>
        /// Gets the left operand. This may be a <see cref="ObjectReference"/>, a primitive value, or a nested <see cref="SimpleExpression"/>.
        /// </summary>
        /// <value>The left operand.</value>
        public object LeftOperand
        {
            get { return _leftOperand; }
        }

        /// <summary>
        /// Gets the type of expression represented.
        /// </summary>
        /// <value>The type.</value>
        public SimpleExpressionType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the right operand. This may be a <see cref="ObjectReference"/>, a primitive value, or a nested <see cref="SimpleExpression"/>.
        /// </summary>
        /// <value>The right operand.</value>
        public object RightOperand
        {
            get { return _rightOperand; }
        }

        public IEnumerable<object> GetValues()
        {
            return GetValues(_leftOperand).Concat(GetValues(_rightOperand));
        }

        private static IEnumerable<object> GetValues(object operand)
        {
            if (operand == null) return Yield(null);
            if (CommonTypes.Contains(operand.GetType())) return Yield(operand);

            if (operand is SimpleReference)
            {
                return Enumerable.Empty<object>();
            }
            var expression = operand as SimpleExpression;
            if (expression != null)
            {
                return expression.GetValues();
            }
            var function = operand as SimpleFunction;
            if (function != null)
            {
                return function.Args;
            }
            return Yield(operand);
        }

        private static IEnumerable<object> Yield(object value)
        {
            yield return value;
        }

        /// <summary>
        /// Implements the operator &amp;. In Simple.Data, this is used as a standard AND operator in place of &&.
        /// </summary>
        /// <param name="left">The left expression.</param>
        /// <param name="right">The right expression.</param>
        /// <returns>A new <see cref="SimpleExpression"/> combining both expressions with an AND operator.</returns>
        public static SimpleExpression operator &(SimpleExpression left, SimpleExpression right)
        {
            return new SimpleExpression(left, right, SimpleExpressionType.And);
        }


        /// <summary>
        /// Implements the operator |. In Simple.Data, this is used as a standard OR operator in place of ||.
        /// </summary>
        /// <param name="left">The left expression.</param>
        /// <param name="right">The right expression.</param>
        /// <returns>A new <see cref="SimpleExpression"/> combining both expressions with an OR operator.</returns>
        public static SimpleExpression operator |(SimpleExpression left, SimpleExpression right)
        {
            return new SimpleExpression(left, right, SimpleExpressionType.Or);
        }

        public static bool operator true(SimpleExpression foo)
        {
            return false;
        }

        public static bool operator false(SimpleExpression foo)
        {
            return false;
        }
    }
}
