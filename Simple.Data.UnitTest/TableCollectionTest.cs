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
            Helper("Users", "Users");
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [TestMethod()]
        public void FindCaseInsensitiveMatchTest()
        {
            Helper("USERS", "Users");
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [TestMethod()]
        public void FindPluralMatchTest()
        {
            Helper("Users", "User");
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [TestMethod()]
        public void FindSingularMatchTest()
        {
            Helper("User", "Users");
        }

        [TestMethod]
        public void FindSnakeFromCamel()
        {
            Helper("USER_PROFILE", "UserProfile");
        }

        [TestMethod]
        public void FindSnakeFromPluralCamel()
        {
            Helper("USER_PROFILE", "UserProfiles");
        }

        [TestMethod]
        public void FindCamelFromSnake()
        {
            Helper("UserProfile", "USER_PROFILE");
        }

        [TestMethod]
        public void FindCamelFromPluralSnake()
        {
            Helper("UserProfiles", "USER_PROFILE");
        }

        private static void Helper(string actualName, string searchName)
        {
            var target = new TableCollection { new Table(actualName, "", "BASE TABLE", null) };
            var actual = target.Find(searchName);
            Assert.IsNotNull(actual);
            Assert.AreEqual(actualName, actual.ActualName);
        }
    }
}
