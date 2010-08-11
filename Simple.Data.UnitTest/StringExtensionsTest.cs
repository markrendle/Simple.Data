using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Simple.Data.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for StringExtensionsTest and is intended
    ///to contain all StringExtensionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StringExtensionsTest
    {
        /// <summary>
        ///A test for IsPlural
        ///</summary>
        [TestMethod]
        public void IsPluralLowercaseUsersShouldReturnTrue()
        {
            Assert.IsTrue("Users".IsPlural());
        }

        /// <summary>
        ///A test for IsPlural
        ///</summary>
        [TestMethod]
        public void IsPluralUppercaseUsersShouldReturnTrue()
        {
            Assert.IsTrue("USERS".IsPlural());
        }

        /// <summary>
        ///A test for IsPlural
        ///</summary>
        [TestMethod]
        public void IsPluralLowercaseUsersShouldReturnFalse()
        {
            Assert.IsFalse("User".IsPlural());
        }

        /// <summary>
        ///A test for IsPlural
        ///</summary>
        [TestMethod]
        public void IsPluralUppercaseUserShouldReturnFalse()
        {
            Assert.IsFalse("USER".IsPlural());
        }

        /// <summary>
        ///A test for Pluralize
        ///</summary>
        [TestMethod()]
        public void PluralizeUserShouldReturnUsers()
        {
            Assert.AreEqual("Users", "User".Pluralize());
        }

        /// <summary>
        ///A test for Pluralize
        ///</summary>
        [TestMethod()]
// ReSharper disable InconsistentNaming
        public void PluralizeUSERShouldReturnUSERS()
// ReSharper restore InconsistentNaming
        {
            Assert.AreEqual("USERS", "USER".Pluralize());
        }

        /// <summary>
        ///A test for Singularize
        ///</summary>
        [TestMethod()]
        public void SingularizeUsersShouldReturnUser()
        {
            Assert.AreEqual("User", "Users".Singularize());
        }

        /// <summary>
        ///A test for Singularize
        ///</summary>
        [TestMethod()]
        public void SingularizeUserShouldReturnUser()
        {
            Assert.AreEqual("User", "User".Singularize());
        }

        /// <summary>
        ///A test for Singularize
        ///</summary>
        [TestMethod()]
// ReSharper disable InconsistentNaming
        public void SingularizeUSERSShouldReturnUSER()
// ReSharper restore InconsistentNaming
        {
            Assert.AreEqual("USER", "USERS".Singularize());
        }

        /// <summary>
        ///A test for IsAllUpperCase
        ///</summary>
        [TestMethod()]
        public void IsAllUpperCaseTrueTest()
        {
            Assert.IsTrue("USERS".IsAllUpperCase());
        }

        /// <summary>
        ///A test for IsAllUpperCase
        ///</summary>
        [TestMethod()]
        public void IsAllUpperCaseProperFalseTest()
        {
            Assert.IsFalse("Users".IsAllUpperCase());
        }

        /// <summary>
        ///A test for IsAllUpperCase
        ///</summary>
        [TestMethod()]
        public void IsAllUpperCasePascalFalseTest()
        {
            Assert.IsFalse("MoreUsers".IsAllUpperCase());
        }

        /// <summary>
        ///A test for IsAllUpperCase
        ///</summary>
        [TestMethod()]
        public void IsAllUpperCaseLowerFalseTest()
        {
            Assert.IsFalse("users".IsAllUpperCase());
        }

        /// <summary>
        ///A test for IsAllUpperCase
        ///</summary>
        [TestMethod()]
        public void IsAllUpperCaseMixedFalseTest()
        {
            Assert.IsFalse("CUSTOMEr".IsAllUpperCase());
        }
    }
}
