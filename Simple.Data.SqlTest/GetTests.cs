using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class GetTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestGet()
        {
var db = DatabaseHelper.Open();
var user = db.Users.Get(1);
Assert.AreEqual(1, user.Id);
        }
    }
}