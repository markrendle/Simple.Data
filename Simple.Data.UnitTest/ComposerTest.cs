namespace Shitty.Data.UnitTest
{
    using NUnit.Framework;

    [TestFixture]
    public class ComposerTest
    {
        [Test]
        public void SetDefaultComposerWorks()
        {
            var stub = new StubComposer();
            Composer.SetDefault(stub);
            Assert.AreSame(stub, Composer.Default);
        }
    }
}
