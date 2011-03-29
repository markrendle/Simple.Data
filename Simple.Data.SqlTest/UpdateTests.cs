using System.Dynamic;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class UpdateTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestUpdateWithNamedArguments()
        {
            var db = DatabaseHelper.Open();

            db.Users.UpdateById(Id: 1, Name: "Ford", Password: "hoopy", Age: 29);
            var user = db.Users.FindById(1);
            Assert.IsNotNull(user);
            Assert.AreEqual("Ford", user.Name);
            Assert.AreEqual("hoopy", user.Password);
            Assert.AreEqual(29, user.Age);
        }

        [Test]
        public void TestUpdateWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var user = new User { Id = 2, Name = "Zaphod", Password = "zarquon", Age = 42 };

            db.Users.Update(user);

            User actual = db.Users.FindById(2);

            Assert.IsNotNull(user);
            Assert.AreEqual("Zaphod", actual.Name);
            Assert.AreEqual("zarquon", actual.Password);
            Assert.AreEqual(42, actual.Age);
        }

        [Test]
        public void TestUpdateWithDynamicTypeObject()
        {
            var db = DatabaseHelper.Open();

            dynamic user = new ExpandoObject();
            user.Id = 3;
            user.Name = "Marvin";
            user.Password = "diodes";
            user.Age = 42000000;

            db.Users.Update(user);

            var actual = db.Users.FindById(3);

            Assert.IsNotNull(user);
            Assert.AreEqual("Marvin", actual.Name);
            Assert.AreEqual("diodes", actual.Password);
            Assert.AreEqual(42000000, actual.Age);
        }
    }
}