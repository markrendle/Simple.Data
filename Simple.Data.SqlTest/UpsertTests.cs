namespace Simple.Data.SqlTest
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using NUnit.Framework;
    using Resources;

    [TestFixture]
    public class UpsertTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestUpsertWithNamedArgumentsAndExistingObject()
        {
            var db = DatabaseHelper.Open();

            db.Users.UpsertById(Id: 1, Name: "Ford Prefect");
            var user = db.Users.Get(1);

            Assert.IsNotNull(user);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("Ford Prefect", user.Name);
        }

        [Test]
        public void TestUpsertWithNamedArgumentsAndExistingObjectUsingTransaction()
        {
            using (var tx = DatabaseHelper.Open().BeginTransaction())
            {

                tx.Users.UpsertById(Id: 1, Name: "Ford Prefect");
                var user = tx.Users.Get(1);
                tx.Commit();

                Assert.IsNotNull(user);
                Assert.AreEqual(1, user.Id);
                Assert.AreEqual("Ford Prefect", user.Name);
            }
        }

        [Test]
        public void TestUpsertWithNamedArgumentsAndNewObject()
        {
            var db = DatabaseHelper.Open();

            var user = db.Users.UpsertById(Id: 0, Name: "Ford Prefect", Password: "Foo", Age: 42);

            Assert.IsNotNull(user);
            Assert.AreNotEqual(0, user.Id);
            Assert.AreEqual("Ford Prefect", user.Name);
            Assert.AreEqual("Foo", user.Password);
            Assert.AreEqual(42, user.Age);
        }

        [Test]
        public void TestUpsertWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var user = new User {Id = 2, Name = "Charlie", Password = "foobar", Age = 42};

            var actual = db.Users.Upsert(user);

            Assert.IsNotNull(user);
            Assert.AreEqual(2, actual.Id);
            Assert.AreEqual("Charlie", actual.Name);
            Assert.AreEqual("foobar", actual.Password);
            Assert.AreEqual(42, actual.Age);
        }

        [Test]
        public void TestUpsertByWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var user = new User {Id = 2, Name = "Charlie", Password = "foobar", Age = 42};

            var actual = db.Users.UpsertById(user);

            Assert.IsNotNull(user);
            Assert.AreEqual(2, actual.Id);
            Assert.AreEqual("Charlie", actual.Name);
            Assert.AreEqual("foobar", actual.Password);
            Assert.AreEqual(42, actual.Age);
        }

        [Test]
        public void TestMultiUpsertWithStaticTypeObjectsForExistingRecords()
        {
            var db = DatabaseHelper.Open();

            var users = new[]
                            {
                                new User { Id = 1, Name = "Slartibartfast", Password = "bistromathics", Age = 777 },
                                new User { Id = 2, Name = "Wowbagger", Password = "teatime", Age = int.MaxValue }
                            };

            IList<User> actuals = db.Users.Upsert(users).ToList<User>();

            Assert.AreEqual(2, actuals.Count);
            Assert.AreEqual(1, actuals[0].Id);
            Assert.AreEqual("Slartibartfast", actuals[0].Name);
            Assert.AreEqual("bistromathics", actuals[0].Password);
            Assert.AreEqual(777, actuals[0].Age);

            Assert.AreEqual(2, actuals[1].Id);
            Assert.AreEqual("Wowbagger", actuals[1].Name);
            Assert.AreEqual("teatime", actuals[1].Password);
            Assert.AreEqual(int.MaxValue, actuals[1].Age);
        }

        [Test]
        public void TestMultiUpsertWithStaticTypeObjectsForNewRecords()
        {
            var db = DatabaseHelper.Open();

            var users = new[]
                            {
                                new User { Name = "Slartibartfast", Password = "bistromathics", Age = 777 },
                                new User { Name = "Wowbagger", Password = "teatime", Age = int.MaxValue }
                            };

            IList<User> actuals = db.Users.Upsert(users).ToList<User>();

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
        public void TestMultiUpsertWithStaticTypeObjectsForMixedRecords()
        {
            var db = DatabaseHelper.Open();

            var users = new[]
                            {
                                new User { Id = 1, Name = "Slartibartfast", Password = "bistromathics", Age = 777 },
                                new User { Name = "Wowbagger", Password = "teatime", Age = int.MaxValue }
                            };

            IList<User> actuals = db.Users.Upsert(users).ToList<User>();

            Assert.AreEqual(2, actuals.Count);
            Assert.AreEqual(1, actuals[0].Id);
            Assert.AreEqual("Slartibartfast", actuals[0].Name);
            Assert.AreEqual("bistromathics", actuals[0].Password);
            Assert.AreEqual(777, actuals[0].Age);

            Assert.AreNotEqual(0, actuals[1].Id);
            Assert.AreEqual("Wowbagger", actuals[1].Name);
            Assert.AreEqual("teatime", actuals[1].Password);
            Assert.AreEqual(int.MaxValue, actuals[1].Age);
        }

        [Test]
        public void TestMultiUpsertWithStaticTypeObjectsAndNoReturn()
        {
            var db = DatabaseHelper.Open();

            var users = new[]
                            {
                                new User { Name = "Slartibartfast", Password = "bistromathics", Age = 777 },
                                new User { Name = "Wowbagger", Password = "teatime", Age = int.MaxValue }
                            };

            //IList<User> actuals = db.Users.Upsert(users).ToList<User>();
            db.Users.Upsert(users);

            var slartibartfast = db.Users.FindByName("Slartibartfast");
            Assert.IsNotNull(slartibartfast);
            Assert.AreNotEqual(0, slartibartfast.Id);
            Assert.AreEqual("Slartibartfast", slartibartfast.Name);
            Assert.AreEqual("bistromathics", slartibartfast.Password);
            Assert.AreEqual(777, slartibartfast.Age);

            var wowbagger = db.Users.FindByName("Wowbagger");
            Assert.IsNotNull(wowbagger);

            Assert.AreNotEqual(0, wowbagger.Id);
            Assert.AreEqual("Wowbagger", wowbagger.Name);
            Assert.AreEqual("teatime", wowbagger.Password);
            Assert.AreEqual(int.MaxValue, wowbagger.Age);
        }

        [Test]
        public void TestUpsertWithDynamicTypeObject()
        {
            var db = DatabaseHelper.Open();

            dynamic user = new ExpandoObject();
            user.Name = "Marvin";
            user.Password = "diodes";
            user.Age = 42000000;

            var actual = db.Users.Upsert(user);

            Assert.IsNotNull(user);
            Assert.AreEqual("Marvin", actual.Name);
            Assert.AreEqual("diodes", actual.Password);
            Assert.AreEqual(42000000, actual.Age);
        }

        [Test]
        public void TestMultiUpsertWithDynamicTypeObjects()
        {
            var db = DatabaseHelper.Open();

            dynamic user1 = new ExpandoObject();
            user1.Name = "Slartibartfast";
            user1.Password = "bistromathics";
            user1.Age = 777;

            dynamic user2 = new ExpandoObject();
            user2.Name = "Wowbagger";
            user2.Password = "teatime";
            user2.Age = int.MaxValue;

            var users = new[] { user1, user2 };

            IList<dynamic> actuals = db.Users.Upsert(users).ToList();

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
        public void TestMultiUpsertWithErrorCallback()
        {
            var db = DatabaseHelper.Open();

            dynamic user1 = new ExpandoObject();
            user1.Name = "Slartibartfast";
            user1.Password = "bistromathics";
            user1.Age = 777;

            dynamic user2 = new ExpandoObject();
            user2.Name = null;
            user2.Password = null;
            user2.Age = null;

            dynamic user3 = new ExpandoObject();
            user3.Name = "Wowbagger";
            user3.Password = "teatime";
            user3.Age = int.MaxValue;

            var users = new[] { user1, user2, user3 };
            bool passed = false;

            ErrorCallback onError = (o, exception) => passed = true;

            IList<dynamic> actuals = db.Users.Upsert(users,onError).ToList();

            Assert.IsTrue(passed, "Callback was not called.");
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
        public void TestMultiUpsertWithErrorCallbackUsingTransaction()
        {
            IList<dynamic> actuals;
            bool passed = false;
            using (var tx = DatabaseHelper.Open().BeginTransaction())
            {
                dynamic user1 = new ExpandoObject();
                user1.Name = "Slartibartfast";
                user1.Password = "bistromathics";
                user1.Age = 777;

                dynamic user2 = new ExpandoObject();
                user2.Name = null;
                user2.Password = null;
                user2.Age = null;

                dynamic user3 = new ExpandoObject();
                user3.Name = "Wowbagger";
                user3.Password = "teatime";
                user3.Age = int.MaxValue;

                var users = new[] {user1, user2, user3};

                ErrorCallback onError = (o, exception) => passed = true;

                actuals = tx.Users.Upsert(users, onError).ToList();
            }

            Assert.IsTrue(passed, "Callback was not called.");
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
        public void TestTransactionMultiUpsertWithErrorCallback()
        {
            var db = DatabaseHelper.Open();
            IList<dynamic> actuals;
            bool passed = false;
            using (var tx = db.BeginTransaction())
            {
                dynamic user1 = new ExpandoObject();
                user1.Name = "Slartibartfast";
                user1.Password = "bistromathics";
                user1.Age = 777;

                dynamic user2 = new ExpandoObject();
                user2.Name = null;
                user2.Password = null;
                user2.Age = null;

                dynamic user3 = new ExpandoObject();
                user3.Name = "Wowbagger";
                user3.Password = "teatime";
                user3.Age = int.MaxValue;

                var users = new[] {user1, user2, user3};

                ErrorCallback onError = (o, exception) => passed = true;

                actuals = db.Users.Upsert(users, onError).ToList();

                tx.Commit();
            }

            Assert.IsTrue(passed, "Callback was not called.");
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
        public void TestWithImageColumn()
        {
            var db = DatabaseHelper.Open();
            try
            {
                var image = GetImage.Image;
                db.Images.Upsert(Id: 1, TheImage: image);
                var img = (DbImage)db.Images.FindById(1);
                Assert.IsTrue(image.SequenceEqual(img.TheImage));
            }
            finally
            {
                db.Images.DeleteById(1);
            }
        }

        [Test]
        public void TestUpsertWithVarBinaryMaxColumn()
        {
            var db = DatabaseHelper.Open();
            var image = GetImage.Image;
            var blob = new Blob
                           {
                               Id = 1,
                               Data = image
                           };
            db.Blobs.Upsert(blob);
            blob = db.Blobs.FindById(1);
            Assert.IsTrue(image.SequenceEqual(blob.Data));
        }

        [Test]
        public void TestUpsertWithSingleArgumentAndExistingObject()
        {
            var db = DatabaseHelper.Open();

            var actual = db.Users.UpsertById(Id: 1);

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Id);
            Assert.IsNotNull(actual.Name);
        }
    }
}