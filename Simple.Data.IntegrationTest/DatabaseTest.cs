using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestClass]
    public class DatabaseTest
    {
        static Database CreateDatabase()
        {
            return new Database(new MockConnectionProvider());
        }

        static Database CreateDatabaseWithDummyData()
        {
            return new Database(new MockConnectionProvider((new MockDbConnection() { DummyDataTable = CreateDummyDataTable()})));
        }

        [TestMethod]
        public void TestFindByDynamicSingleColumn()
        {
            dynamic database = CreateDatabase();
            database.Users.FindByName("Foo");
            Assert.AreEqual("select Users.* from Users where Users.name like @p1", MockDatabase.Sql, true);
            Assert.AreEqual("Foo", MockDatabase.Parameters[0]);
        }

        [TestMethod]
        public void TestFindByDynamicTwoColumns()
        {
            dynamic database = CreateDatabase();
            database.Users.FindByNameAndPassword("Foo", "secret");
            Assert.AreEqual("select Users.* from Users where (Users.name like @p1 and Users.password like @p2)", MockDatabase.Sql, true);
            Assert.AreEqual("Foo", MockDatabase.Parameters[0]);
            Assert.AreEqual("secret", MockDatabase.Parameters[1]);
        }

        [TestMethod]
        public void TestFindAllByDynamic()
        {
            dynamic database = CreateDatabase();
            database.Users.FindAllByName("Foo");
            Assert.AreEqual("select Users.* from Users where Users.name like @p1", MockDatabase.Sql, true);
            Assert.AreEqual("Foo", MockDatabase.Parameters[0]);
        }

        [TestMethod]
        public void TestInsertWithNamedArguments()
        {
            dynamic database = CreateDatabase();
            database.Users.Insert(Name: "Steve", Age: 50);
            Assert.AreEqual("insert into Users (Name,Age) values (@p0,@p1)", MockDatabase.Sql, true);
            Assert.AreEqual("Steve", MockDatabase.Parameters[0]);
            Assert.AreEqual(50, MockDatabase.Parameters[1]);
        }

        [TestMethod]
        public void TestUpdateWithNamedArguments()
        {
            dynamic database = CreateDatabase();
            database.Users.UpdateById(Id: 1, Name: "Steve", Age: 50);
            Assert.AreEqual("update Users set Name = @p0, Age = @p1 where Id = @p2", MockDatabase.Sql, true);
            Assert.AreEqual("Steve", MockDatabase.Parameters[0]);
            Assert.AreEqual(50, MockDatabase.Parameters[1]);
            Assert.AreEqual(1, MockDatabase.Parameters[2]);
        }

        [TestMethod]
        public void TestDeleteWithNamedArguments()
        {
            dynamic database = CreateDatabase();
            database.Users.Delete(Id: 1);
            Assert.AreEqual("delete from Users where Id = @p0", MockDatabase.Sql, true);
            Assert.AreEqual(1, MockDatabase.Parameters[0]);
        }

        [TestMethod]
        public void TestInsertOnTable()
        {
            dynamic person = new ExpandoObject();
            person.Name = "Phil";
            person.Age = 42;
            dynamic database = CreateDatabase();
            database.Users.Insert(person);
            Assert.AreEqual("insert into Users (Name,Age) values (@p0,@p1)", MockDatabase.Sql, true);
            Assert.AreEqual("Phil", MockDatabase.Parameters[0]);
            Assert.AreEqual(42, MockDatabase.Parameters[1]);
        }

        [TestMethod]
        public void TestStronglyTypedQuery()
        {
            dynamic database = CreateDatabaseWithDummyData();
            User user = database.Users.FindByName("Bob");
            Assert.AreEqual("Bob", user.Name);
            Assert.AreEqual("Secret", user.Password);
            Assert.AreEqual(42, user.Age);
        }

        [TestMethod]
        public void TestAllPropertyShouldWriteDeprecatedMessageToTrace()
        {
            var traceListener = new TestTraceListener();
            Trace.Listeners.Add(traceListener);

            dynamic database = CreateDatabaseWithDummyData();
            foreach (var user in database.Users.All)
            {
                Assert.AreEqual("Bob", user.Name);
                Assert.AreEqual("Secret", user.Password);
                Assert.AreEqual(42, user.Age);
                Assert.IsTrue(traceListener.Messages.Contains("deprecated"));
            }

            Trace.Listeners.Remove(traceListener);
        }

        [TestMethod]
        public void TestAll()
        {
            dynamic database = CreateDatabaseWithDummyData();
            foreach (var user in database.Users.All())
            {
                Assert.AreEqual("Bob", user.Name);
                Assert.AreEqual("Secret", user.Password);
                Assert.AreEqual(42, user.Age);
            }
        }

        [TestMethod]
        public void TestStronglyTypedAll()
        {
            dynamic database = CreateDatabaseWithDummyData();
            foreach (User user in database.Users.All)
            {
                Assert.AreEqual("Bob", user.Name);
                Assert.AreEqual("Secret", user.Password);
                Assert.AreEqual(42, user.Age);
            }
        }

        private static DataTable CreateDummyDataTable()
        {
            var table = new DataTable("Users");
            table.Columns.Add("Name", typeof (string));
            table.Columns.Add("Password", typeof(string));
            table.Columns.Add("Age", typeof(int));

            table.LoadDataRow(new object[] {"Bob", "Secret", 42}, true);

            return table;
        }
    }
}
