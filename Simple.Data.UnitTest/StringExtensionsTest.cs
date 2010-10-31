using System;
using NUnit.Framework;
using Simple.Data.Extensions;

namespace Simple.Data.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for StringExtensionsTest and is intended
    ///to contain all StringExtensionsTest Unit Tests
    ///</summary>
    [TestFixture()]
    public class StringExtensionsTest
    {
        /// <summary>
        ///A test for IsPlural
        ///</summary>
        [Test]
        public void IsPluralLowercaseUsersShouldReturnTrue()
        {
            Assert.IsTrue("Users".IsPlural());
        }

        /// <summary>
        ///A test for IsPlural
        ///</summary>
        [Test]
        public void IsPluralUppercaseUsersShouldReturnTrue()
        {
            Assert.IsTrue("USERS".IsPlural());
        }

        /// <summary>
        ///A test for IsPlural
        ///</summary>
        [Test]
        public void IsPluralLowercaseUsersShouldReturnFalse()
        {
            Assert.IsFalse("User".IsPlural());
        }

        /// <summary>
        ///A test for IsPlural
        ///</summary>
        [Test]
        public void IsPluralUppercaseUserShouldReturnFalse()
        {
            Assert.IsFalse("USER".IsPlural());
        }

        /// <summary>
        ///A test for Pluralize
        ///</summary>
        [Test()]
        public void PluralizeUserShouldReturnUsers()
        {
            Assert.AreEqual("Users", "User".Pluralize());
        }

        /// <summary>
        ///A test for Pluralize
        ///</summary>
        [Test()]
// ReSharper disable InconsistentNaming
        public void PluralizeUSERShouldReturnUSERS()
// ReSharper restore InconsistentNaming
        {
            Assert.AreEqual("USERS", "USER".Pluralize());
        }

        /// <summary>
        ///A test for Singularize
        ///</summary>
        [Test()]
        public void SingularizeUsersShouldReturnUser()
        {
            Assert.AreEqual("User", "Users".Singularize());
        }

        /// <summary>
        ///A test for Singularize
        ///</summary>
        [Test()]
        public void SingularizeUserShouldReturnUser()
        {
            Assert.AreEqual("User", "User".Singularize());
        }

        /// <summary>
        ///A test for Singularize
        ///</summary>
        [Test()]
// ReSharper disable InconsistentNaming
        public void SingularizeUSERSShouldReturnUSER()
// ReSharper restore InconsistentNaming
        {
            Assert.AreEqual("USER", "USERS".Singularize());
        }

        /// <summary>
        ///A test for IsAllUpperCase
        ///</summary>
        [Test()]
        public void IsAllUpperCaseTrueTest()
        {
            Assert.IsTrue("USERS".IsAllUpperCase());
        }

        /// <summary>
        ///A test for IsAllUpperCase
        ///</summary>
        [Test()]
        public void IsAllUpperCaseProperFalseTest()
        {
            Assert.IsFalse("Users".IsAllUpperCase());
        }

        /// <summary>
        ///A test for IsAllUpperCase
        ///</summary>
        [Test()]
        public void IsAllUpperCasePascalFalseTest()
        {
            Assert.IsFalse("MoreUsers".IsAllUpperCase());
        }

        /// <summary>
        ///A test for IsAllUpperCase
        ///</summary>
        [Test()]
        public void IsAllUpperCaseLowerFalseTest()
        {
            Assert.IsFalse("users".IsAllUpperCase());
        }

        /// <summary>
        ///A test for IsAllUpperCase
        ///</summary>
        [Test()]
        public void IsAllUpperCaseMixedFalseTest()
        {
            Assert.IsFalse("CUSTOMEr".IsAllUpperCase());
        }
    }
}
