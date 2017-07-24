namespace Shitty.Data.UnitTest
{
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class SimpleRecordConvertTest
    {
        [Test]
        public void CanConvertToFoo()
        {
            dynamic source = new SimpleRecord(new Dictionary<string, object> {{"X", "Bar"}});
            Foo actual = source;
            Assert.AreEqual("Bar", actual.X);
        }
        
        [Test]
        public void CanConvertWithSubItemToFoo()
        {
            dynamic source = new SimpleRecord(new Dictionary<string, object> {{"X", "Bar"}, {"Y", new Dictionary<string,object> { {"X", "Quux"}}}});
            Foo actual = source;
            Assert.AreEqual("Bar", actual.X);
            Assert.IsNotNull(actual.Y);
            Assert.AreEqual("Quux", actual.Y.X);
        }

        [Test]
        public void CanConvertWithSubItemAndCollectionToFoo()
        {
            dynamic source =
                new SimpleRecord(new Dictionary<string, object>
                                     {{"X", "Bar"},
                                     {"Y", new Dictionary<string, object> {{"X", "Quux"}}},
                                     {"Z", new[] { new Dictionary<string, object> {{"X", "Wibble"}}}}
                                     });
            Foo actual = source;
            Assert.AreEqual("Bar", actual.X);
            Assert.IsNotNull(actual.Y);
            Assert.AreEqual("Quux", actual.Y.X);
            Assert.IsNotNull(actual.Z);
            Assert.AreEqual(1, actual.Z.Count);
            Assert.AreEqual("Wibble", actual.Z.Single().X);
        }

        public class Foo
        {
            public string X { get; set; }
            public Foo Y { get; set; }
            public ICollection<Foo> Z { get; set; }
        }
    }
}
