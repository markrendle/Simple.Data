using System;
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
        /// Gets the left operand. This may be a <see cref="DynamicReference"/>, a primitive value, or a nested <see cref="SimpleExpression"/>.
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
        /// Gets the right operand. This may be a <see cref="DynamicReference"/>, a primitive value, or a nested <see cref="SimpleExpression"/>.
        /// </summary>
        /// <value>The right operand.</value>
        public object RightOperand
        {
            get { return _rightOperand; }
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
    }
}
