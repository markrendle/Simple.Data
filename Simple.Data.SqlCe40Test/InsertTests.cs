using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlCe40Test
{
    using System.Dynamic;
    using System.IO;
    using System.Reflection;
    using NUnit.Framework;
    using SqlCeTest;

    [TestFixture]
    public class InsertTests
    {
        private static readonly string DatabasePath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)),
                    "TestDatabase.sdf");
        
        [Test]
        public void TestInsertWithNamedArguments()
        {
            var db = Database.Opener.OpenFile(DatabasePath);

            var user = db.Users.Insert(Name: "Ford", Password: "hoopy", Age: 29);

            Assert.IsNotNull(user);
            Assert.AreEqual("Ford", user.Name);
            Assert.AreEqual("hoopy", user.Password);
            Assert.AreEqual(29, user.Age);
        }

        [Test]
        public void TestInsertWithStaticTypeObject()
        {
            var db = Database.Opener.OpenFile(DatabasePath);

            var user = new User { Name = "Zaphod", Password = "zarquon", Age = 42 };

            var actual = db.Users.Insert(user);

            Assert.IsNotNull(user);
            Assert.AreEqual("Zaphod", actual.Name);
            Assert.AreEqual("zarquon", actual.Password);
            Assert.AreEqual(42, actual.Age);
        }

        [Test]
        public void TestMultiInsertWithStaticTypeObjects()
        {
            var db = Database.Opener.OpenFile(DatabasePath);

            var users = new[]
                            {
                                new User { Name = "Slartibartfast", Password = "bistromathics", Age = 777 },
                                new User { Name = "Wowbagger", Password = "teatime", Age = int.MaxValue }
                            };

            IList<User> actuals = db.Users.Insert(users).ToList<User>();

            Assert.AreEqual(2, actuals.Count);
            Assert.AreNotEqual(0, actuals[0].Id);
            Assert.AreEqual("Slartibartfast", actuals[0].Name);
            Assert.AreEqual("bistromathics", actuals[0].Password);
            Assert.AreEqual(777, actuals[0].Age);

            Assert.AreNotEqual(0, actuals[1].Id);
            Assert.AreEqual("Wowbagger", actuals[1].Name);
            Assert.AreEqual("teatime", actuals[1].Password);
            Assert.AreEqual(int.MaxValue, actuals[1].Age);
        }

        [Test]
        public void TestInsertWithDynamicTypeObject()
        {
            var db = Database.Opener.OpenFile(DatabasePath);

            dynamic user = new ExpandoObject();
            user.Name = "Marvin";
            user.Password = "diodes";
            user.Age = 42000000;

            var actual = db.Users.Insert(user);

            Assert.IsNotNull(user);
            Assert.AreEqual("Marvin", actual.Name);
            Assert.AreEqual("diodes", actual.Password);
            Assert.AreEqual(42000000, actual.Age);
        }

        [Test]
        public void TestMultiInsertWithDynamicTypeObjects()
        {
            var db = Database.Opener.OpenFile(DatabasePath);

            dynamic user1 = new ExpandoObject();
            user1.Name = "Prak";
            user1.Password = "truth";
            user1.Age = 30;

            dynamic user2 = new ExpandoObject();
            user2.Name = "Eddie";
            user2.Password = "tea";
            user2.Age = 1;

            var users = new[] { user1, user2 };

            IList<dynamic> actuals = db.Users.Insert(users).ToList();

            Assert.AreEqual(2, actuals.Count);
            Assert.AreNotEqual(0, actuals[0].Id);
            Assert.AreEqual("Prak", actuals[0].Name);
            Assert.AreEqual("truth", actuals[0].Password);
            Assert.AreEqual(30, actuals[0].Age);

            Assert.AreNotEqual(0, actuals[1].Id);
            Assert.AreEqual("Eddie", actuals[1].Name);
            Assert.AreEqual("tea", actuals[1].Password);
            Assert.AreEqual(1, actuals[1].Age);
        }
    }
}
