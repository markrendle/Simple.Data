using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Simple.Data.UnitTest
{
    [TestClass]
    public class SimpleExpressionTest
    {
        // ReSharper disable InconsistentNaming
        [TestMethod]
        public void BitwiseAndOperatorCombinesTwoExpressions()
        {
            // Arrange
            var expr1 = new SimpleExpression(1, 1, SimpleExpressionType.Equal);
            var expr2 = new SimpleExpression(2, 2, SimpleExpressionType.Equal);

            // Act
            var newExpr = expr1 & expr2;

            // Assert
            Assert.AreEqual(expr1, newExpr.LeftOperand);
            Assert.AreEqual(expr2, newExpr.RightOperand);
            Assert.AreEqual(SimpleExpressionType.And, newExpr.Type);
        }

        [TestMethod]
        public void BitwiseOrOperatorCombinesTwoExpressions()
        {
            // Arrange
            var expr1 = new SimpleExpression(1, 1, SimpleExpressionType.Equal);
            var expr2 = new SimpleExpression(2, 2, SimpleExpressionType.Equal);

            // Act
            var newExpr = expr1 | expr2;

            // Assert
            Assert.AreEqual(expr1, newExpr.LeftOperand);
            Assert.AreEqual(expr2, newExpr.RightOperand);
            Assert.AreEqual(SimpleExpressionType.Or, newExpr.Type);
        }

        [TestMethod]
        public void CompoundOperatorsRespectParentheses_AndOrAnd()
        {
            // Arrange
            var expr1 = new SimpleExpression(1, 1, SimpleExpressionType.Equal);
            var expr2 = new SimpleExpression(2, 2, SimpleExpressionType.Equal);
            var expr3 = new SimpleExpression(3, 3, SimpleExpressionType.Equal);
            var expr4 = new SimpleExpression(4, 4, SimpleExpressionType.Equal);

            // Act
            var newExpr = (expr1 & expr2) | (expr3 & expr4);
            var leftExpr = newExpr.LeftOperand as SimpleExpression;
            var rightExpr = newExpr.RightOperand as SimpleExpression;

            // Assert
            Assert.IsNotNull(leftExpr);
            Assert.IsNotNull(rightExpr);
            Assert.AreEqual(newExpr.Type, SimpleExpressionType.Or);
            Assert.AreEqual(expr1, leftExpr.LeftOperand);
            Assert.AreEqual(expr2, leftExpr.RightOperand);
            Assert.AreEqual(expr3, rightExpr.LeftOperand);
            Assert.AreEqual(expr4, rightExpr.RightOperand);
        }


        [TestMethod]
        public void CompoundExpressionsEvaluateAndOperatorsFirst()
        {
            // Arrange
            var expr1 = new SimpleExpression(1, 1, SimpleExpressionType.Equal);
            var expr2 = new SimpleExpression(2, 2, SimpleExpressionType.Equal);
            var expr3 = new SimpleExpression(3, 3, SimpleExpressionType.Equal);
            var expr4 = new SimpleExpression(4, 4, SimpleExpressionType.Equal);

            // Act
            var newExpr = expr1 & expr2 | expr3 & expr4;
            var leftExpr = newExpr.LeftOperand as SimpleExpression;
            var rightExpr = newExpr.RightOperand as SimpleExpression;

            // Assert
            Assert.IsNotNull(leftExpr);
            Assert.IsNotNull(rightExpr);
            Assert.AreEqual(newExpr.Type, SimpleExpressionType.Or);
            Assert.AreEqual(expr1, leftExpr.LeftOperand);
            Assert.AreEqual(expr2, leftExpr.RightOperand);
            Assert.AreEqual(expr3, rightExpr.LeftOperand);
            Assert.AreEqual(expr4, rightExpr.RightOperand);
        }

        private static void CompoundExpressionEvaluationOrderHelper(Func<SimpleExpression,SimpleExpression,SimpleExpression,SimpleExpression,SimpleExpression> actor)
        {
            // Arrange
            var expr1 = new SimpleExpression(1, 1, SimpleExpressionType.Equal);
            var expr2 = new SimpleExpression(2, 2, SimpleExpressionType.Equal);
            var expr3 = new SimpleExpression(3, 3, SimpleExpressionType.Equal);
            var expr4 = new SimpleExpression(4, 4, SimpleExpressionType.Equal);

            // Act
            var actual = actor(expr1, expr2, expr3, expr4);

            // Assert
            Assert.AreEqual(expr4, GetExpression(actual, 0).RightOperand);
            Assert.AreEqual(expr3, GetExpression(actual, 1).RightOperand);
            Assert.AreEqual(expr2, GetExpression(actual, 2).RightOperand);
            Assert.AreEqual(expr1, GetExpression(actual, 2).LeftOperand);
        }

        /// <summary>
        /// Checks the order in which expressions are combined.
        /// Where the expression is A & B & C & D, the grouping should be (((A & B) & C) & D)
        /// </summary>
        [TestMethod]
        public void CompoundExpressionsEvaluateRightToLeft_AndAndAnd()
        {
            CompoundExpressionEvaluationOrderHelper((e1,e2,e3,e4) => e1 & e2 & e3 & e4);
        }

        /// <summary>
        /// Checks the order in which expressions are combined.
        /// Where the expression is A | B | C | D, the grouping should be (((A | B) | C) | D)
        /// </summary>
        [TestMethod]
        public void CompoundExpressionsEvaluateRightToLeft_OrOrOr()
        {
            CompoundExpressionEvaluationOrderHelper((e1, e2, e3, e4) => e1 | e2 | e3 | e4);
        }

        private static SimpleExpression GetExpression(SimpleExpression expression, int nestLevel)
        {
            for (int i = 0; i < nestLevel; i++)
            {
                expression = (SimpleExpression)expression.LeftOperand;
            }
            return expression;
        }
        // ReSharper restore InconsistentNaming
    }
}
