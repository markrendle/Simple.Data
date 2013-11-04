namespace Simple.Data.UnitTest
{
    using NUnit.Framework;

    [TestFixture]
    public class SimpleRecordCloneTest
    {
        [Test]
        public void CloneShouldNotBeSameObject()
        {
            dynamic target = new SimpleRecord();
            var actual = target.Clone();
            Assert.AreNotSame(target, actual);
        }

        [Test]
        public void CloneShouldContainSameValues()
        {
            dynamic target = new SimpleRecord();
            target.Name = "Foo";
            var actual = target.Clone();
            Assert.AreNotSame(target, actual);
            Assert.AreEqual(target.Name, actual.Name);
        }

        [Test]
        public void CloneShouldNotChangeWhenOriginalChanges()
        {
            dynamic target = new SimpleRecord();
            target.Name = "Foo";
            var actual = target.Clone();
            target.Name = "Bar";
            Assert.AreEqual("Foo", actual.Name);
        }
    }
}