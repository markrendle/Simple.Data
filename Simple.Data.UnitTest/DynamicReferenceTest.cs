using Simple.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Simple.Data.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for DynamicTableOrColumnTest and is intended
    ///to contain all DynamicTableOrColumnTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DynamicReferenceTest
    {
        [TestMethod]
        public void GetDynamicPropertyReturnsNewDynamicReferenceWithTableAndColumn()
        {
            // Arrange
            dynamic table = new DynamicReference("Table");

            // Act
            DynamicReference column = table.Column;

            // Assert
            Assert.AreEqual("Column", column.Name);
            Assert.AreEqual("Table", column.Owner.Name);
        }

        [TestMethod]
        public void GetDynamicPropertyDotPropertyReturnsNewDynamicReferenceWithTwoOwners()
        {
            // Arrange
            dynamic table = new DynamicReference("Table1");

            // Act
            DynamicReference column = table.Table2.Column;

            // Assert
            Assert.AreEqual("Column", column.Name);
            Assert.AreEqual("Table2", column.Owner.Name);
            Assert.AreEqual("Table1", column.Owner.Owner.Name);
        }

        /// <summary>
        ///A test for GetAllObjectNames
        ///</summary>
        [TestMethod()]
        public void GetAllObjectNamesTest()
        {
            // Arrange
            dynamic table = new DynamicTable("One", null);
            var column = table.Two.Column;

            // Act
            string[] names = column.GetAllObjectNames();

            // Assert
            Assert.AreEqual("One", names[0]);
            Assert.AreEqual("Two", names[1]);
            Assert.AreEqual("Column", names[2]);
        }

        [TestMethod]
        public void FromStringTest()
        {
            // Act
            var actual = DynamicReference.FromString("One.Two.Three");

            // Assert
            Assert.AreEqual("Three", actual.Name);
            Assert.AreEqual("Two", actual.Owner.Name);
            Assert.AreEqual("One", actual.Owner.Owner.Name);
            Assert.IsNull(actual.Owner.Owner.Owner);
        }

        private static void DoAsserts<T>(SimpleExpression expression, DynamicReference column, T rightOperand, SimpleExpressionType expressionType)
        {
            Assert.AreEqual(column, expression.LeftOperand);
            Assert.AreEqual(rightOperand, expression.RightOperand);
            Assert.AreEqual(expressionType, expression.Type);
        }

        [TestMethod]
        public void EqualOperatorReturnsSimpleExpressionWithEqualType()
        {
            // Arrange
            var column = DynamicReference.FromStrings("foo", "bar");

            // Act
            var expression = column == 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.Equal);
        }

        [TestMethod]
        public void EqualOperatorReturnsSimpleExpressionWithEqualTypeWhenUsedAsDynamic()
        {
            // Arrange
            dynamic column = DynamicReference.FromStrings("foo", "bar");

            // Act
            var expression = column == 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.Equal);
        }

        [TestMethod]
        public void NotEqualOperatorReturnsSimpleExpressionWithNotEqualType()
        {
            // Arrange
            var column = DynamicReference.FromStrings("foo", "bar");

            // Act
            var expression = column != 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.NotEqual);
        }

        [TestMethod]
        public void LessThanOperatorReturnsSimpleExpressionWithLessThanType()
        {
            // Arrange
            var column = DynamicReference.FromStrings("foo", "bar");

            // Act
            var expression = column < 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.LessThan);
        }

        [TestMethod]
        public void LessThanOrEqualOperatorReturnsSimpleExpressionWithLessThanOrEqualType()
        {
            // Arrange
            var column = DynamicReference.FromStrings("foo", "bar");

            // Act
            var expression = column <= 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.LessThanOrEqual);
        }

        [TestMethod]
        public void GreaterThanOperatorReturnsSimpleExpressionWithGreaterThanType()
        {
            // Arrange
            var column = DynamicReference.FromStrings("foo", "bar");

            // Act
            var expression = column > 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.GreaterThan);
        }

        [TestMethod]
        public void GreaterThanOrEqualOperatorReturnsSimpleExpressionWithGreaterThanOrEqualType()
        {
            // Arrange
            var column = DynamicReference.FromStrings("foo", "bar");

            // Act
            var expression = column >= 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.GreaterThanOrEqual);
        }
    }
}
