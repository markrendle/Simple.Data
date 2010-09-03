using Simple.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Simple.Data.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for MethodNameParserTest and is intended
    ///to contain all MethodNameParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MethodNameParserTest
    {
        /// <summary>
        ///A test for GetColumns
        ///</summary>
        [TestMethod()]
        public void GetColumnsTest()
        {
            const string methodName = "ThisAndThat";
            IList<string> expected = new[] {"This", "That"};
            IList<string> actual = MethodNameParser.GetColumns(methodName);
            ListHelper.AssertAreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ParseFromMethodName
        ///</summary>
        [TestMethod()]
        public void ParseFromMethodNameTest()
        {
            const string methodName = "FindByThisAndThat";
            IList<object> args = new object[] {1, "Foo"};
            IDictionary<string, object> expected = new Dictionary<string, object> { {"This", 1}, {"That", "Foo"}};
            IDictionary<string, object> actual = MethodNameParser.ParseFromMethodName(methodName, expected);
            ListHelper.AssertAreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveCommandPart
        ///</summary>
        [TestMethod()]
        public void RemoveCommandPartFindByTest()
        {
            RemoveCommandPartHelper("ThisAndThat", "FindByThisAndThat");
        }

        [TestMethod()]
        public void RemoveCommandPartFindAllByTest()
        {
            RemoveCommandPartHelper("ThisAndThat", "FindAllByThisAndThat");
        }

        [TestMethod()]
        public void RemoveCommandPartUpdateByTest()
        {
            RemoveCommandPartHelper("ThisAndThat", "UpdateByThisAndThat");
        }

        [TestMethod()]
        public void RemoveCommandPartDeleteByTest()
        {
            RemoveCommandPartHelper("ThisAndThat", "DeleteByThisAndThat");
        }
        
        private static void RemoveCommandPartHelper(string expected, string test)
        {
            Assert.AreEqual(expected, MethodNameParser.RemoveCommandPart(test));
        }
    }
}
