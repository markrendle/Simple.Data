using Simple.Data;
using NUnit.Framework;
using System;

namespace Simple.Data.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for DynamicTableOrColumnTest and is intended
    ///to contain all DynamicTableOrColumnTest Unit Tests
    ///</summary>
    [TestFixture()]
    public class DynamicReferenceTest
    {
        [Test]
        public void GetDynamicPropertyReturnsNewDynamicReferenceWithTableAndColumn()
        {
            // Arrange
            dynamic table = new ObjectReference("Table");

            // Act
            ObjectReference column = table.Column;

            // Assert
            Assert.AreEqual("Column", column.GetName());
            Assert.AreEqual("Table", column.GetOwner().GetName());
        }

        [Test]
        public void GetDynamicPropertyDotPropertyReturnsNewDynamicReferenceWithTwoOwners()
        {
            // Arrange
            dynamic table = new ObjectReference("Table1");

            // Act
            ObjectReference column = table.Table2.Column;

            // Assert
            Assert.AreEqual("Column", column.GetName());
            Assert.AreEqual("Table2", column.GetOwner().GetName());
            Assert.AreEqual("Table1", column.GetOwner().GetOwner().GetName());
        }

        /// <summary>
        ///A test for GetAllObjectNames
        ///</summary>
        [Test()]
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

        [Test]
        public void FromStringTest()
        {
            // Act
            var actual = ObjectReference.FromString("One.Two.Three");

            // Assert
            Assert.AreEqual("Three", actual.GetName());
            Assert.AreEqual("Two", actual.GetOwner().GetName());
            Assert.AreEqual("One", actual.GetOwner().GetOwner().GetName());
            Assert.IsNull(actual.GetOwner().GetOwner().GetOwner());
        }

        private static void DoAsserts<T>(SimpleExpression expression, ObjectReference column, T rightOperand, SimpleExpressionType expressionType)
        {
            Assert.AreEqual(column, expression.LeftOperand);
            Assert.AreEqual(rightOperand, expression.RightOperand);
            Assert.AreEqual(expressionType, expression.Type);
        }

        [Test]
        public void EqualOperatorReturnsSimpleExpressionWithEqualType()
        {
            // Arrange
            var column = ObjectReference.FromStrings("foo", "bar");

            // Act
            var expression = column == 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.Equal);
        }

        [Test]
        public void EqualOperatorReturnsSimpleExpressionWithEqualTypeWhenUsedAsDynamic()
        {
            // Arrange
            dynamic column = ObjectReference.FromStrings("foo", "bar");

            // Act
            var expression = column == 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.Equal);
        }

        [Test]
        public void NotEqualOperatorReturnsSimpleExpressionWithNotEqualType()
        {
            // Arrange
            var column = ObjectReference.FromStrings("foo", "bar");

            // Act
            var expression = column != 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.NotEqual);
        }

        [Test]
        public void LessThanOperatorReturnsSimpleExpressionWithLessThanType()
        {
            // Arrange
            var column = ObjectReference.FromStrings("foo", "bar");

            // Act
            var expression = column < 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.LessThan);
        }

        [Test]
        public void LessThanOrEqualOperatorReturnsSimpleExpressionWithLessThanOrEqualType()
        {
            // Arrange
            var column = ObjectReference.FromStrings("foo", "bar");

            // Act
            var expression = column <= 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.LessThanOrEqual);
        }

        [Test]
        public void GreaterThanOperatorReturnsSimpleExpressionWithGreaterThanType()
        {
            // Arrange
            var column = ObjectReference.FromStrings("foo", "bar");

            // Act
            var expression = column > 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.GreaterThan);
        }

        [Test]
        public void GreaterThanOrEqualOperatorReturnsSimpleExpressionWithGreaterThanOrEqualType()
        {
            // Arrange
            var column = ObjectReference.FromStrings("foo", "bar");

            // Act
            var expression = column >= 1;

            // Assert
            DoAsserts(expression, column, 1, SimpleExpressionType.GreaterThanOrEqual);
        }
    }
}
