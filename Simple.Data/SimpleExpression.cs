using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Simple.Data
{
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

        public object LeftOperand
        {
            get { return _leftOperand; }
        }

        public SimpleExpressionType Type
        {
            get { return _type; }
        }

        public object RightOperand
        {
            get { return _rightOperand; }
        }

        public static SimpleExpression operator &(SimpleExpression left, SimpleExpression right)
        {
            return new SimpleExpression(left, right, SimpleExpressionType.And);
        }


        public static SimpleExpression operator |(SimpleExpression left, SimpleExpression right)
        {
            return new SimpleExpression(left, right, SimpleExpressionType.Or);
        }
    }
}
