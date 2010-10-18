using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Simple.Data.UnitTest
{
    [TestClass]
    public class DynamicColumnTest
    {
        private static void DoAsserts<T>(SimpleExpression expression, DynamicColumn column, T rightOperand, SimpleExpressionType expressionType)
        {
            Assert.AreEqual(column, expression.LeftOperand);
            Assert.AreEqual(rightOperand, expression.RightOperand);
            Assert.AreEqual(expressionType, expression.Type);
        }

        [TestMethod]
        public void EqualOperatorReturnsSimpleExpressionWithEqualType()
        {
            // Arrange
            var column = new DynamicColumn("foo", "bar");

            // Act
            var expression = column == 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.Equal);
        }

        [TestMethod]
        public void NotEqualOperatorReturnsSimpleExpressionWithNotEqualType()
        {
            // Arrange
            var column = new DynamicColumn("foo", "bar");

            // Act
            var expression = column != 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.NotEqual);
        }

        [TestMethod]
        public void LessThanOperatorReturnsSimpleExpressionWithLessThanType()
        {
            // Arrange
            var column = new DynamicColumn("foo", "bar");

            // Act
            var expression = column < 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.LessThan);
        }

        [TestMethod]
        public void LessThanOrEqualOperatorReturnsSimpleExpressionWithLessThanOrEqualType()
        {
            // Arrange
            var column = new DynamicColumn("foo", "bar");

            // Act
            var expression = column <= 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.LessThanOrEqual);
        }

        [TestMethod]
        public void GreaterThanOperatorReturnsSimpleExpressionWithGreaterThanType()
        {
            // Arrange
            var column = new DynamicColumn("foo", "bar");

            // Act
            var expression = column > 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.GreaterThan);
        }

        [TestMethod]
        public void GreaterThanOrEqualOperatorReturnsSimpleExpressionWithGreaterThanOrEqualType()
        {
            // Arrange
            var column = new DynamicColumn("foo", "bar");

            // Act
            var expression = column >= 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.GreaterThanOrEqual);
        }
    }
}
