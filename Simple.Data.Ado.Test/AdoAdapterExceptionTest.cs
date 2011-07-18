namespace Simple.Data.Ado.Test
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using NUnit.Framework;

    [TestFixture]
    public class AdoAdapterExceptionTest
    {
        [Test]
        public void EmptyConstructorShouldSetAdapterType()
        {
            var actual = new AdoAdapterException();
            Assert.AreEqual(typeof(AdoAdapter), actual.AdapterType);
        }

        [Test]
        public void SingleStringConstructorShouldSetMessageAndAdapterType()
        {
            var actual = new AdoAdapterException("Foo");
            Assert.AreEqual(typeof(AdoAdapter), actual.AdapterType);
            Assert.AreEqual("Foo", actual.Message);
        }

        [Test]
        public void StringAndExceptionConstructorShouldSetMessageAndInnerExceptionAndAdapterType()
        {
            var inner = new Exception();
            var actual = new AdoAdapterException("Foo", inner);
            Assert.AreEqual(typeof(AdoAdapter), actual.AdapterType);
            Assert.AreEqual("Foo", actual.Message);
            Assert.AreSame(inner, actual.InnerException);
        }

        [Test]
        public void StringAndDbCommandConstructorShouldSetMessageAndCommandTextAndAdapterType()
        {
            var command = new SqlCommand("Bar");
            var actual = new AdoAdapterException("Foo", command);
            Assert.AreEqual(typeof(AdoAdapter), actual.AdapterType);
            Assert.AreEqual("Foo", actual.Message);
            Assert.AreEqual(command.CommandText, actual.CommandText);
        }

        [Test]
        public void StringAndDictionaryConstructorShouldSetCommandTextAndParametersAndAdapterType()
        {
            var param = new Dictionary<string, object> { { "P", "quux" } };
            var actual = new AdoAdapterException("Foo", param);
            Assert.AreEqual(typeof(AdoAdapter), actual.AdapterType);
            Assert.AreEqual("Foo", actual.CommandText);
            Assert.AreEqual("quux", actual.Parameters["P"]);
        }

        [Test]
        public void StringAndStringAndDictionaryConstructorShouldSetMessageAndCommandTextAndParametersAndAdapterType()
        {
            var param = new Dictionary<string, object> { { "P", "quux" } };
            var actual = new AdoAdapterException("Foo", "Bar", param);
            Assert.AreEqual(typeof(AdoAdapter), actual.AdapterType);
            Assert.AreEqual("Foo", actual.Message);
            Assert.AreEqual("Bar", actual.CommandText);
            Assert.AreEqual("quux", actual.Parameters["P"]);
        }
    }
}
