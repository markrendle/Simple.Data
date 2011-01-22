using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestFixture]
    public class RangeAndArrayFindTest
    {
        static Database CreateDatabase(MockDatabase mockDatabase)
        {
            var mockSchemaProvider = new MockSchemaProvider();
            mockSchemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" });
            mockSchemaProvider.SetColumns(new[] { "dbo", "Users", "Id" },
                                          new[] { "dbo", "Users", "Name" },
                                          new[] { "dbo", "Users", "Password" },
                                          new[] { "dbo", "Users", "Age" },
                                          new[] { "dbo", "Users", "JoinDate"});
            mockSchemaProvider.SetPrimaryKeys(new object[] { "dbo", "Users", "Id", 0 });
            return new Database(new AdoAdapter(new MockConnectionProvider(new MockDbConnection(mockDatabase), mockSchemaProvider)));
        }

        [Test]
        public void TestFindByWithIntRange()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.FindAllById(1.to(10)).ToList();
            Assert.AreEqual("select [dbo].[users].* from [dbo].[users] where [dbo].[users].[id] between (@p1 and @p2)".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, mockDatabase.Parameters[0]);
            Assert.AreEqual(10, mockDatabase.Parameters[1]);
        }

        [Test]
        public void TestFindByWithIntArray()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.FindAllById(new[] { 1, 2, 3 }).ToList();
            Assert.AreEqual("select [dbo].[users].* from [dbo].[users] where [dbo].[users].[id] in (@p1,@p2,@p3)".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(1, mockDatabase.Parameters[0]);
            Assert.AreEqual(2, mockDatabase.Parameters[1]);
            Assert.AreEqual(3, mockDatabase.Parameters[2]);
        }

        [Test]
        public void TestFindByWithDateRange()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.FindAllByJoinDate("01/01/2011".to("31/01/2011")).ToList();
            Assert.AreEqual("select [dbo].[users].* from [dbo].[users] where [dbo].[users].[joindate] between (@p1 and @p2)".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            Assert.AreEqual(new DateTime(2011,1,1), mockDatabase.Parameters[0]);
            Assert.AreEqual(new DateTime(2011,1,31), mockDatabase.Parameters[1]);
        }
    }
}
