namespace Simple.Data.SqlTest
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Resources;
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class UpsertTests
    {
        public UpsertTests()
        {
            DatabaseHelper.Reset();
        }

        //TODO: [Fact]
        public async void TestUpsertWithNamedArgumentsAndExistingObject()
        {
            var db = DatabaseHelper.Open();

            await db.Users.UpsertById(Id: 1, Name: "Ford Prefect");
            var user = await db.Users.FindById(1);

            Assert.IsNotNull(user);
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("Ford Prefect", user.Name);
        }

        //TODO: [Fact]
        public async void TestUpsertWithNamedArgumentsAndExistingObjectUsingTransaction()
        {
            using (var tx = DatabaseHelper.Open().BeginTransaction())
            {

                await tx.Users.UpsertById(Id: 1, Name: "Ford Prefect");
                var user = await tx.Users.FindById(1);
                tx.Commit();

                Assert.IsNotNull(user);
                Assert.AreEqual(1, user.Id);
                Assert.AreEqual("Ford Prefect", user.Name);
            }
        }

        //TODO: [Fact]
        public async void TestUpsertWithNamedArgumentsAndNewObject()
        {
            var db = DatabaseHelper.Open();

            var user = await db.Users.UpsertById(Id: 0, Name: "Ford Prefect", Password: "Foo", Age: 42);

            Assert.IsNotNull(user);
            Assert.AreNotEqual(0, user.Id);
            Assert.AreEqual("Ford Prefect", user.Name);
            Assert.AreEqual("Foo", user.Password);
            Assert.AreEqual(42, user.Age);
        }

        [Fact]
        public async void TestUpsertWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var user = new User {Id = 2, Name = "Charlie", Password = "foobar", Age = 42};

            var actual = await db.Users.Upsert(user);

            Assert.IsNotNull(user);
            Assert.AreEqual(2, actual.Id);
            Assert.AreEqual("Charlie", actual.Name);
            Assert.AreEqual("foobar", actual.Password);
            Assert.AreEqual(42, actual.Age);
        }

        //TODO: [Fact]
        public async void TestUpsertByWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var user = new User {Id = 2, Name = "Charlie", Password = "foobar", Age = 42};

            var actual = await db.Users.UpsertById(user);

            Assert.IsNotNull(user);
            Assert.AreEqual(2, actual.Id);
            Assert.AreEqual("Charlie", actual.Name);
            Assert.AreEqual("foobar", actual.Password);
            Assert.AreEqual(42, actual.Age);
        }

        [Fact]
        public async void TestMultiUpsertWithStaticTypeObjectsForExistingRecords()
        {
            var db = DatabaseHelper.Open();

            var users = new[]
                            {
                                new User { Id = 1, Name = "Slartibartfast", Password = "bistromathics", Age = 777 },
                                new User { Id = 2, Name = "Wowbagger", Password = "teatime", Age = int.MaxValue }
                            };

            IList<User> actuals = await db.Users.Upsert(users).ToList<User>();

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

        [Fact]
        public async void TestMultiUpsertWithStaticTypeObjectsForNewRecords()
        {
            var db = DatabaseHelper.Open();

            var users = new[]
                            {
                                new User { Name = "Slartibartfast", Password = "bistromathics", Age = 777 },
                                new User { Name = "Wowbagger", Password = "teatime", Age = int.MaxValue }
                            };

            IList<User> actuals = await db.Users.Upsert(users).ToList<User>();

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

        [Fact]
        public async void TestMultiUpsertWithStaticTypeObjectsForMixedRecords()
        {
            var db = DatabaseHelper.Open();

            var users = new[]
                            {
                                new User { Id = 1, Name = "Slartibartfast", Password = "bistromathics", Age = 777 },
                                new User { Name = "Wowbagger", Password = "teatime", Age = int.MaxValue }
                            };

            IList<User> actuals = await db.Users.Upsert(users).ToList<User>();

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

        [Fact]
        public async void TestMultiUpsertWithStaticTypeObjectsAndNoReturn()
        {
            var db = DatabaseHelper.Open();

            var users = new[]
                            {
                                new User { Name = "Slartibartfast", Password = "bistromathics", Age = 777 },
                                new User { Name = "Wowbagger", Password = "teatime", Age = int.MaxValue }
                            };

            //IList<User> actuals = db.Users.Upsert(users).ToList<User>();
            await db.Users.Upsert(users);

            var slartibartfast = await db.Users.FindByName("Slartibartfast");
            Assert.IsNotNull(slartibartfast);
            Assert.AreNotEqual(0, slartibartfast.Id);
            Assert.AreEqual("Slartibartfast", slartibartfast.Name);
            Assert.AreEqual("bistromathics", slartibartfast.Password);
            Assert.AreEqual(777, slartibartfast.Age);

            var wowbagger = await db.Users.FindByName("Wowbagger");
            Assert.IsNotNull(wowbagger);

            Assert.AreNotEqual(0, wowbagger.Id);
            Assert.AreEqual("Wowbagger", wowbagger.Name);
            Assert.AreEqual("teatime", wowbagger.Password);
            Assert.AreEqual(int.MaxValue, wowbagger.Age);
        }

        [Fact]
        public async void TestUpsertWithDynamicTypeObject()
        {
            var db = DatabaseHelper.Open();

            dynamic user = new ExpandoObject();
            user.Name = "Marvin";
            user.Password = "diodes";
            user.Age = 42000000;

            var actual = await db.Users.Upsert(user);

            Assert.IsNotNull(user);
            Assert.AreEqual("Marvin", actual.Name);
            Assert.AreEqual("diodes", actual.Password);
            Assert.AreEqual(42000000, actual.Age);
        }

        [Fact]
        public async void TestMultiUpsertWithDynamicTypeObjects()
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

            IList<dynamic> actuals = await db.Users.Upsert(users).ToList();

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

        [Fact]
        public async void TestMultiUpsertWithErrorCallback()
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

            IList<dynamic> actuals = await db.Users.Upsert(users,onError).ToList();

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

        [Fact]
        public async void TestMultiUpsertWithErrorCallbackUsingTransaction()
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

                actuals = await tx.Users.Upsert(users, onError).ToList();
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

        [Fact]
        public async void TestTransactionMultiUpsertWithErrorCallback()
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

                actuals = await db.Users.Upsert(users, onError).ToList();

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

        [Fact]
        public async void TestWithImageColumn()
        {
            var db = DatabaseHelper.Open();
            try
            {
                var image = GetImage.Image;
                await db.Images.Upsert(Id: 1, TheImage: image);
                var img = (DbImage)(await db.Images.FindById(1));
                Assert.IsTrue(image.SequenceEqual(img.TheImage));
            }
            finally
            {
                db.Images.DeleteById(1);
            }
        }

        [Fact]
        public async void TestUpsertWithVarBinaryMaxColumn()
        {
            var db = DatabaseHelper.Open();
            var image = GetImage.Image;
            var blob = new Blob
                           {
                               Id = 1,
                               Data = image
                           };
            await db.Blobs.Upsert(blob);
            blob = await db.Blobs.FindById(1);
            Assert.IsTrue(image.SequenceEqual(blob.Data));
        }

        //TODO: [Fact]
        public async void TestUpsertWithSingleArgumentAndExistingObject()
        {
            var db = DatabaseHelper.Open();

            var actual = await db.Users.UpsertById(Id: 1);

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Id);
            Assert.IsNotNull(actual.Name);
        }

        //TODO: [Fact]
        public async void TestUpsertUserBySecondaryField()
        {
          var db = DatabaseHelper.Open();

          var id = await db.Users.UpsertByName(new User() { Age = 20, Name = "Black sheep", Password = "Bah" }).Id;
          User actual = await db.Users.FindById(id);

          Assert.AreEqual(id, actual.Id);
          Assert.AreEqual("Black sheep", actual.Name);
          Assert.AreEqual("Bah", actual.Password);
          Assert.AreEqual(20, actual.Age);
        }

        //TODO: [Fact]
        public async void TestUpsertUserByTwoSecondaryFields()
        {
          var db = DatabaseHelper.Open();

          var id = await db.Users.UpsertByNameAndPassword(new User() { Age = 20, Name = "Black sheep", Password = "Bah" }).Id;
          User actual = await db.Users.FindById(id);

          Assert.AreEqual(id, actual.Id);
          Assert.AreEqual("Black sheep", actual.Name);
          Assert.AreEqual("Bah", actual.Password);
          Assert.AreEqual(20, actual.Age);
        }

        //TODO: [Fact]
        public async void TestUpsertExisting()
        {
          var db = DatabaseHelper.Open();

          var id = await db.Users.UpsertByNameAndPassword(new User() { Age = 20, Name = "Black sheep", Password = "Bah" }).Id;
          await db.Users.UpsertById(new User() { Id = id, Age = 12, Name = "Dog", Password = "Bark" });

          User actual = await db.Users.FindById(id);

          Assert.AreEqual(id, actual.Id);
          Assert.AreEqual("Dog", actual.Name);
          Assert.AreEqual("Bark", actual.Password);
          Assert.AreEqual(12, actual.Age);
        }
    }
}