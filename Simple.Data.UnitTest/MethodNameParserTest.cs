using NUnit.Framework;
using Simple.Data;
using System;
using System.Collections.Generic;

namespace Simple.Data.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for MethodNameParserTest and is intended
    ///to contain all MethodNameParserTest Unit Tests
    ///</summary>
    [TestFixture()]
    public class MethodNameParserTest
    {
        /// <summary>
        ///A test for GetColumns
        ///</summary>
        [Test()]
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
        [Test()]
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
        [Test()]
        public void RemoveCommandPartFindByTest()
        {
            RemoveCommandPartHelper("ThisAndThat", "FindByThisAndThat");
        }

        [Test()]
        public void RemoveCommandPartFindAllByTest()
        {
            RemoveCommandPartHelper("ThisAndThat", "FindAllByThisAndThat");
        }

        [Test()]
        public void RemoveCommandPartUpdateByTest()
        {
            RemoveCommandPartHelper("ThisAndThat", "UpdateByThisAndThat");
        }

        [Test()]
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
