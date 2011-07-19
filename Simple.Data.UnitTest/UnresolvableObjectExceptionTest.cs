namespace Simple.Data.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using NUnit.Framework;

    public class UnresolvableObjectExceptionTest
    {
        [Test]
        public void EmptyConstructor()
        {
            var actual = new UnresolvableObjectException();
            Assert.IsNotNull(actual);
            Assert.IsNullOrEmpty(actual.ObjectName);
        }

        [Test]
        public void SingleStringSetsObjectName()
        {
            var actual = new UnresolvableObjectException("Foo");
            Assert.AreEqual("Foo", actual.ObjectName);
        }

        [Test]
        public void TwoStringsSetsObjectNameAndMessage()
        {
            var actual = new UnresolvableObjectException("Foo", "Bar");
            Assert.AreEqual("Foo", actual.ObjectName);
            Assert.AreEqual("Bar", actual.Message);
        }

        [Test]
        public void TwoStringsAndAnExceptionSetsObjectNameAndMessageAndInnerException()
        {
            var inner = new Exception();
            var actual = new UnresolvableObjectException("Foo", "Bar", inner);
            Assert.AreEqual("Foo", actual.ObjectName);
            Assert.AreEqual("Bar", actual.Message);
            Assert.AreSame(inner, actual.InnerException);
        }
    }
}