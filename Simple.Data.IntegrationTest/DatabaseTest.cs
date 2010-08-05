using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.IntegrationTest.Stubs;

namespace Simple.Data.IntegrationTest
{
    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void TestFindByDynamicSingleColumn()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.Users.FindByName("Foo");
            Assert.AreEqual("select * from Users where name = @p0", DatabaseStub.Sql);
            Assert.AreEqual("Foo", DatabaseStub.Parameters[0]);
        }

        [TestMethod]
        public void TestFindByDynamicTwoColumns()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.Users.FindByNameAndPassword("Foo", "secret");
            Assert.AreEqual("select * from Users where name = @p0 and password = @p1", DatabaseStub.Sql);
            Assert.AreEqual("Foo", DatabaseStub.Parameters[0]);
            Assert.AreEqual("secret", DatabaseStub.Parameters[1]);
        }

        [TestMethod]
        public void TestFindAllByDynamic()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.Users.FindAllByName("Foo");
            Assert.AreEqual("select * from Users where name = @p0", DatabaseStub.Sql);
            Assert.AreEqual("Foo", DatabaseStub.Parameters[0]);
        }

        [TestMethod]
        public void TestQuery()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.Query("select * from Users where name = ? and age > ?", "Bob", 35);
            Assert.AreEqual("select * from Users where name = @p0 and age > @p1", DatabaseStub.Sql);
            Assert.AreEqual("Bob", DatabaseStub.Parameters[0]);
            Assert.AreEqual(35, DatabaseStub.Parameters[1]);
        }

        [TestMethod]
        public void TestExecuteWithInsert()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.Execute("insert into Users values (?,?)", "Bob", 35);
            Assert.AreEqual("insert into Users values (@p0,@p1)", DatabaseStub.Sql);
            Assert.AreEqual("Bob", DatabaseStub.Parameters[0]);
            Assert.AreEqual(35, DatabaseStub.Parameters[1]);
        }

        [TestMethod]
        public void TestExecuteWithUpdate()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.Execute("update Users set name = ?, age = ? where id = ?", "Bob", 35, 1);
            Assert.AreEqual("update Users set name = @p0, age = @p1 where id = @p2", DatabaseStub.Sql);
            Assert.AreEqual("Bob", DatabaseStub.Parameters[0]);
            Assert.AreEqual(35, DatabaseStub.Parameters[1]);
            Assert.AreEqual(1, DatabaseStub.Parameters[2]);
        }

        [TestMethod]
        public void TestNamedArgumentInsertOnTable()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.Users.Insert(Name: "Steve", Age: 50);
            Assert.AreEqual("insert into Users (Name,Age) values (@p0,@p1)", DatabaseStub.Sql);
            Assert.AreEqual("Steve", DatabaseStub.Parameters[0]);
            Assert.AreEqual(50, DatabaseStub.Parameters[1]);
        }

        [TestMethod]
        public void TestInsertOnTable()
        {
            dynamic person = new ExpandoObject();
            person.Name = "Phil";
            person.Age = 42;
            dynamic database = new Database(new DbConnectionStub());
            database.Users.Insert(person);
            Assert.AreEqual("insert into Users (Name,Age) values (@p0,@p1)", DatabaseStub.Sql);
            Assert.AreEqual("Phil", DatabaseStub.Parameters[0]);
            Assert.AreEqual(42, DatabaseStub.Parameters[1]);
        }

        [TestMethod]
        public void TestStronglyTypedQuery()
        {
            dynamic database = new Database(new DbConnectionStub {DummyDataTable = CreateDummyDataTable()});
            User user = database.Users.FindByName("Bob");
            Assert.AreEqual("Bob", user.Name);
            Assert.AreEqual("Secret", user.Password);
            Assert.AreEqual(42, user.Age);
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
