// ReSharper disable InconsistentNaming
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace Simple.Data.Mocking.Test
{
    
    
    /// <summary>
    ///This is a test class for XmlStubAdapterTest and is intended
    ///to contain all XmlStubAdapterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class XmlMockAdapterTest
    {
        private dynamic _db;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            MockHelper.UseMockAdapter(
                new XmlMockAdapter(
                    @"<Root><Users Id=""System.Int32""><User Id=""1"" Email=""foo"" Password=""bar""/><User Email=""baz"" Password=""quux""/></Users></Root>"));
        }

        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}


        /// <summary>
        ///A test for Find
        ///</summary>
        [TestMethod()]
        public void FindTest()
        {
            var user = Database.Default.Users.FindByEmail("foo");
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("foo", user.Email);
            Assert.AreEqual("bar", user.Password);
        }

        /// <summary>
        ///A test for All
        ///</summary>
        [TestMethod()]
        public void All_ShouldReturnTwoUsers()
        {
            IEnumerable<object> users = Database.Default.Users.All;
            Assert.AreEqual(2, users.Count());
        }
    }
}
// ReSharper restore InconsistentNaming
