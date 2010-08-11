using Simple.Data.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Simple.Data.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for TableCollectionTest and is intended
    ///to contain all TableCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TableCollectionTest
    {
        /// <summary>
        ///A test for Find
        ///</summary>
        [TestMethod()]
        public void FindExactMatchTest()
        {
            const string expected = "Users";
            var target = new TableCollection {new Table(expected, "", null)};
            var actual = target.Find("Users");
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual.ActualName);
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [TestMethod()]
        public void FindCaseInsensitiveMatchTest()
        {
            const string expected = "USERS";
            var target = new TableCollection { new Table(expected, "", null) };
            var actual = target.Find("Users");
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual.ActualName);
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [TestMethod()]
        public void FindPluralMatchTest()
        {
            const string expected = "USERS";
            var target = new TableCollection { new Table(expected, "", null) };
            var actual = target.Find("User");
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual.ActualName);
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [TestMethod()]
        public void FindSingularMatchTest()
        {
            const string expected = "USER";
            var target = new TableCollection { new Table(expected, "", null) };
            var actual = target.Find("Users");
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual.ActualName);
        }
    }
}
