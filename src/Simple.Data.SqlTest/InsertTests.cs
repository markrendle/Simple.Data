using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Simple.Data.Ado;
using Simple.Data.SqlTest.Resources;

namespace Simple.Data.SqlTest
{
    using System;
    using Xunit;
    using Assert = NUnit.Framework.Assert;

    public class InsertTests
    {
        public InsertTests()
        {
            DatabaseHelper.Reset();
        }

        [Fact]
        public async void TestInsertWithNamedArguments()
        {
            var db = DatabaseHelper.Open();

            var user = await db.Users.Insert(Name: "Ford", Password: "hoopy", Age: 29);

            Assert.IsNotNull(user);
            Assert.AreEqual("Ford", user.Name);
            Assert.AreEqual("hoopy", user.Password);
            Assert.AreEqual(29, user.Age);
        }

        [Fact]
        public async void TestInsertWithIdentityInsertOn()
        {
            var db = DatabaseHelper.Open().WithOptions(new AdoOptions(identityInsert: true));
            var user = await db.Users.Insert(Id: 42, Name: "Arthur", Password: "Tea", Age: 30);
            Assert.IsNotNull(user);
            Assert.AreEqual(42, user.Id);
        }

        [Fact]
        public async void TestInsertWithIdentityInsertOnThenOffAgain()
        {
            var db = DatabaseHelper.Open().WithOptions(new AdoOptions(identityInsert: true));
            var user = await db.Users.Insert(Id: 2267709, Name: "Douglas", Password: "dirk", Age: 49);
            Assert.IsNotNull(user);
            Assert.AreEqual(2267709, user.Id);
            db.ClearOptions();
            user = await db.Users.Insert(Name: "Frak", Password: "true", Age: 200);
            Assert.Less(2267709, user.Id);
        }

        [Fact]
        public async void TestInsertWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var user = new User {Name = "Zaphod", Password = "zarquon", Age = 42};

            var actual = await db.Users.Insert(user);

            Assert.IsNotNull(user);
            Assert.AreEqual("Zaphod", actual.Name);
            Assert.AreEqual("zarquon", actual.Password);
            Assert.AreEqual(42, actual.Age);
        }

        [Fact]
        public async void TestMultiInsertWithStaticTypeObjects()
        {
            var db = DatabaseHelper.Open();

            var users = new[]
                            {
                                new User { Name = "Slartibartfast", Password = "bistromathics", Age = 777 },
                                new User { Name = "Wowbagger", Password = "teatime", Age = int.MaxValue }
                            };

            IList<User> actuals = (await db.Users.Insert(users)).ToList<User>();

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
        public async void TestMultiInsertWithStaticTypeObjectsAndNoReturn()
        {
            var db = DatabaseHelper.Open();

            var users = new[]
                            {
                                new User { Name = "Slartibartfast", Password = "bistromathics", Age = 777 },
                                new User { Name = "Wowbagger", Password = "teatime", Age = int.MaxValue }
                            };

            await db.Users.Insert(users);

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
        public async void TestInsertWithDynamicTypeObject()
        {
            var db = DatabaseHelper.Open();

            dynamic user = new ExpandoObject();
            user.Name = "Marvin";
            user.Password = "diodes";
            user.Age = 42000000;

            var actual = await db.Users.Insert(user);

            Assert.IsNotNull(user);
            Assert.AreEqual("Marvin", actual.Name);
            Assert.AreEqual("diodes", actual.Password);
            Assert.AreEqual(42000000, actual.Age);
        }

        [Fact]
        public async void TestMultiInsertWithDynamicTypeObjects()
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

            IList<dynamic> actuals = (await db.Users.Insert(users)).ToList();

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
        public async void TestMultiInsertWithErrorCallback()
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

            IList<dynamic> actuals = (await db.Users.Insert(users,onError)).ToList();

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
        public async void TestTransactionMultiInsertWithErrorCallback()
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

                actuals = (await db.Users.Insert(users, onError)).ToList();

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
                await db.Images.Insert(Id: 1, TheImage: image);
                var img = (DbImage)(await db.Images.FindById(1));
                Assert.IsTrue(image.SequenceEqual(img.TheImage));
            }
            finally
            {
                db.Images.DeleteById(1);
            }
        }

        [Fact]
        public async void TestInsertWithVarBinaryMaxColumn()
        {
            var db = DatabaseHelper.Open();
            var image = GetImage.Image;
            var blob = new Blob
                            {
                                Id = 1,
                                Data = image
                            };
            await db.Blobs.Insert(blob);
            blob = await db.Blobs.FindById(1);
            Assert.IsTrue(image.SequenceEqual(blob.Data));
        }

        [Fact]
        public async void TestInsertWithTimestampColumn()
        {
            var db = DatabaseHelper.Open();
            var row = await db.TimestampTest.Insert(Description: "Foo");
            Assert.IsNotNull(row);
            Assert.IsInstanceOf<byte[]>(row.Version);
        }

        [Fact]
        public async void TestInsertWithDateTimeOffsetColumn()
        {
            var db = DatabaseHelper.Open();
            dynamic entry = new ExpandoObject();
            var time = DateTimeOffset.Now;
            entry.time = time;
            var inserted = await db.DateTimeOffsetTest.Insert(entry);
            Assert.AreEqual(time, inserted.time);
        }
    }
}
