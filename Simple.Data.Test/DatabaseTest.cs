using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple.Data.Test.Stubs;

namespace Simple.Data.Test
{
    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void TestFindByDynamic()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.People.FindByName("Foo");
            Assert.AreEqual("select * from People where name = @p0", DatabaseStub.Sql);
            Assert.AreEqual("Foo", DatabaseStub.Parameters[0]);
        }

        [TestMethod]
        public void TestFindAllByDynamic()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.People.FindAllByName("Foo");
            Assert.AreEqual("select * from People where name = @p0", DatabaseStub.Sql);
            Assert.AreEqual("Foo", DatabaseStub.Parameters[0]);
        }

        [TestMethod]
        public void TestQuery()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.Query("select * from people where name = ? and age > ?", "Bob", 35);
            Assert.AreEqual("select * from people where name = @p0 and age > @p1", DatabaseStub.Sql);
            Assert.AreEqual("Bob", DatabaseStub.Parameters[0]);
            Assert.AreEqual(35, DatabaseStub.Parameters[1]);
        }

        [TestMethod]
        public void TestExecuteWithInsert()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.Execute("insert into people values (?,?)", "Bob", 35);
            Assert.AreEqual("insert into people values (@p0,@p1)", DatabaseStub.Sql);
            Assert.AreEqual("Bob", DatabaseStub.Parameters[0]);
            Assert.AreEqual(35, DatabaseStub.Parameters[1]);
        }

        [TestMethod]
        public void TestExecuteWithUpdate()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.Execute("update people set name = ?, age = ? where id = ?", "Bob", 35, 1);
            Assert.AreEqual("update people set name = @p0, age = @p1 where id = @p2", DatabaseStub.Sql);
            Assert.AreEqual("Bob", DatabaseStub.Parameters[0]);
            Assert.AreEqual(35, DatabaseStub.Parameters[1]);
            Assert.AreEqual(1, DatabaseStub.Parameters[2]);
        }

        [TestMethod]
        public void TestNamedArgumentInsertOnTable()
        {
            dynamic database = new Database(new DbConnectionStub());
            database.People.Insert(Name: "Steve", Age: 50);
            Assert.AreEqual("insert into People (Name,Age) values (@p0,@p1)", DatabaseStub.Sql);
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
            database.People.Insert(person);
            Assert.AreEqual("insert into People (Name,Age) values (@p0,@p1)", DatabaseStub.Sql);
            Assert.AreEqual("Phil", DatabaseStub.Parameters[0]);
            Assert.AreEqual(42, DatabaseStub.Parameters[1]);
        }
    }
}
