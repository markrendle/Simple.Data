using System.Dynamic;
using System.Linq;

namespace Simple.Data.SqlTest
{
    using System.Collections.Generic;
    using Xunit;

    public class UpdateTests
    {
        public UpdateTests()
        {
            DatabaseHelper.Reset();
        }

        [Fact]
        public async void TestUpdateWithNamedArguments()
        {
            var db = DatabaseHelper.Open();

            await db.Users.UpdateById(Id: 1, Name: "Ford", Password: "hoopy", Age: 29);
            var user = await db.Users.FindById(1);
            Assert.NotNull(user);
            Assert.Equal("Ford", user.Name);
            Assert.Equal("hoopy", user.Password);
            Assert.Equal(29, user.Age);
        }

        [Fact]
        public async void TestUpdateWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var user = new User { Id = 2, Name = "Zaphod", Password = "zarquon", Age = 42 };

            await db.Users.Update(user);

            User actual = await db.Users.FindById(2);

            Assert.NotNull(user);
            Assert.Equal("Zaphod", actual.Name);
            Assert.Equal("zarquon", actual.Password);
            Assert.Equal(42, actual.Age);
        }

        [Fact]
        public async void TestUpdateWithDynamicTypeObject()
        {
            var db = DatabaseHelper.Open();

            dynamic user = new ExpandoObject();
            user.Id = 3;
            user.Name = "Marvin";
            user.Password = "diodes";
            user.Age = 42000000;

            await db.Users.Update(user);

            var actual = await db.Users.FindById(3);

            Assert.NotNull(user);
            Assert.Equal("Marvin", actual.Name);
            Assert.Equal("diodes", actual.Password);
            Assert.Equal(42000000, actual.Age);
        }

        [Fact]
        public async void TestUpdateWithVarBinaryMaxColumn()
        {
            var db = DatabaseHelper.Open();
            var blob = new Blob
                           {
                               Id = 1,
                               Data = new byte[] {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
                           };
            await db.Blobs.Insert(blob);

            var newData = blob.Data = new byte[] {0,1,2,3,4,5,6,7,8,9};

            await db.Blobs.Update(blob);

            blob = await db.Blobs.FindById(1);
            
            Assert.True(newData.SequenceEqual(blob.Data));
        }

        //TODO: [Fact]
        public async void TestUpdateWithJoinCriteria()
        {
            var db = DatabaseHelper.Open();
            await db.Customers.UpdateAll(db.Customers.Orders.OrderId == 1, Name: "Updated");
            var customer = await db.Customers.Get(1);
            Assert.Equal("Updated", customer.Name);
        }

        [Fact]
        public async void TestUpdateAllWithNoMatchingRows()
        {
            var db = DatabaseHelper.Open();
            await db.test.SchemaTable.UpdateAll(db.test.SchemaTable.Id == 1138, Description: "Updated");
            var test = await db.test.SchemaTable.FindById(1138);
            Assert.Null(test);
        }

        //TODO: [Fact]
        public async void TestUpdateWithJoinCriteriaOnCompoundKeyTable()
        {
            var db = DatabaseHelper.Open();
            await db.CompoundKeyMaster.UpdateAll(db.CompoundKeyMaster.CompoundKeyDetail.Value == 1, Description: "Updated");
            var record = await db.CompoundKeyMaster.Get(1, 1);
            Assert.Equal("Updated", record.Description);
        }
        
        [Fact]
        public async void ToListShouldExecuteQuery()
        {
            var db = DatabaseHelper.Open();
            List<Customer> customers = await db.Customers.All().ToList<Customer>();
            foreach (var customer in customers)
            {
                customer.Address = "Updated";
            }

            await Assert.DoesNotThrowAsync(async () => await db.Customers.Update(customers));
        }

        //TODO: [Fact]
        public async void TestUpdateWithTimestamp()
        {
            var db = DatabaseHelper.Open();
            var row = await db.TimestampTest.Insert(Description: "Inserted");
            row.Description = "Updated";
            db.TimestampTest.Update(row);
            row = db.TimestampTest.Get(row.Id);
            Assert.Equal("Updated", row.Description);
        }

        [Fact]
        public async void TestUpdateByInputIsNotMutated()
        {
            var db = DatabaseHelper.Open();
            var user = new Dictionary<string, object>
                           {
              {"Id", 0},
              {"Age", 1},
              {"Name", "X"},
              {"Password", "P"}
            };

            user["Id"] = (await db.Users.Insert(user)).Id;

            await db.Users.UpdateById(user);

            Assert.Equal(4, user.Keys.Count);
        }

        [Fact]
        public async void TestUpdatingACriteriaColumn()
        {
            var db = DatabaseHelper.Open();
            var user = await db.Users.Insert(Age: 42, Name: "Z1", Password: "argh");
            await db.Users.UpdateAll(db.Users.Name == "Z1", Name: "1Z");
        }

        [Fact]
        public async void TestUpdateWithOriginalUsingAnonymousObjects()
        {
            var db = DatabaseHelper.Open();
            var user = await db.Users.Insert(Age: 54, Name: "YZ1", Password: "argh");
            await db.Users.Update(new {Name = "2YZ"}, new {Name = "YZ1"});
            var actual = await db.Users.FindById(user.Id);
            Assert.Equal("2YZ", actual.Name);
        }
    }
}