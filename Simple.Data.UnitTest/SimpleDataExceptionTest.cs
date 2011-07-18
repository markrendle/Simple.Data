namespace Simple.Data.UnitTest
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class SimpleDataExceptionTest
    {
        [Test]
        public void EmptyConstructor()
        {
            var actual = new SimpleDataException();
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.Message, "Exception of type 'Simple.Data.SimpleDataException' was thrown.");
        }

        [Test]
        public void StringConstructor()
        {
            var actual = new SimpleDataException("Foo");
            Assert.AreEqual("Foo", actual.Message);
        }

        [Test]
        public void StringAndExceptionConstructor()
        {
            var inner = new Exception();
            var actual = new SimpleDataException("Foo", inner);
            Assert.AreEqual("Foo", actual.Message);
            Assert.AreSame(inner, actual.InnerException);
        }
    }
}
