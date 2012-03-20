using NUnit.Framework;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class UpdateTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
// ReSharper disable CoVariantArrayConversion
            schemaProvider.SetTables(new object[] { "dbo", "Users", "BASE TABLE" },
                                     new object[] { "dbo", "UserHistory", "BASE TABLE"},
                                     new object[] { "dbo", "AnnoyingMaster", "BASE TABLE"},
                                     new[] { "dbo", "AnnoyingDetail", "BASE TABLE"},
                                     new[] {"dbo", "USER_TABLE", "BASE TABLE"});

            schemaProvider.SetColumns(new object[] { "dbo", "Users", "Id", true },
                                      new[] { "dbo", "Users", "Name" },
                                      new[] { "dbo", "Users", "Password" },
                                      new[] { "dbo", "Users", "Age" },
                                      new[] { "dbo", "UserHistory", "Id" },
                                      new[] { "dbo", "UserHistory", "UserId" },
                                      new[] { "dbo", "UserHistory", "LastSeen" },
                                      new[] { "dbo", "AnnoyingMaster", "Id1" },
                                      new[] { "dbo", "AnnoyingMaster", "Id2" },
                                      new[] { "dbo", "AnnoyingMaster", "Text" },
                                      new[] { "dbo", "AnnoyingDetail", "Id" },
                                      new[] { "dbo", "AnnoyingDetail", "MasterId1" },
                                      new[] { "dbo", "AnnoyingDetail", "MasterId2" },
                                      new[] { "dbo", "AnnoyingDetail", "Value" },
                                      new object[] { "dbo", "USER_TABLE", "ID", true },
                                      new[] { "dbo", "USER_TABLE", "NAME" },
                                      new[] { "dbo", "USER_TABLE", "PASSWORD" },
                                      new[] { "dbo", "USER_TABLE", "AGE" });

            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Users", "Id", 0 },
                                          new object[] { "dbo", "UserHistory", "Id", 0 },
                                          new object[] { "dbo", "AnnoyingMaster", "Id1", 0 },
                                          new object[] { "dbo", "AnnoyingMaster", "Id2", 1 },
                                          new object[] { "dbo", "AnnoyingDetail", "Id", 0 },
                                          new object[] { "dbo", "USER_TABLE", "ID", 0 });

            schemaProvider.SetForeignKeys(
                new object[] { "FK_UserHistory_User", "dbo", "UserHistory", "UserId", "dbo", "Users", "Id", 0 },
                new object[] { "FK_AnnoyingDetail_AnnoyingMaster", "dbo", "AnnoyingDetail", "MasterId1", "dbo", "AnnoyingMaster", "Id1", 0 },
                new object[] { "FK_AnnoyingDetail_AnnoyingMaster", "dbo", "AnnoyingDetail", "MasterId2", "dbo", "AnnoyingMaster", "Id2", 1 }
                );
// ReSharper restore CoVariantArrayConversion
        }

        [Test]
        public void TestUpdateWithNamedArguments()
        {
            _db.Users.UpdateById(Id: 1, Name: "Steve", Age: 50);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
            Parameter(2).Is(1);
        }

        [Test]
        public void TestUpdateWithNamedArgumentsUsingExpression()
        {
            _db.Users.UpdateAll(Age: _db.Users.Age + 1);
            GeneratedSqlIs("update [dbo].[Users] set [Age] = [dbo].[Users].[Age] + @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestUpdateWithDynamicObject()
        {
            dynamic record = new SimpleRecord();
            record.Id = 1;
            record.Name = "Steve";
            record.Age = 50;
            _db.Users.Update(record);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
            Parameter(2).Is(1);
        }

        [Test]
        public void TestUpdateWithDynamicObjectAndOriginalValues()
        {
            dynamic newRecord = new SimpleRecord();
            newRecord.Id = 1;
            newRecord.Name = "Steve";
            newRecord.Age = 50;
            dynamic originalRecord = new SimpleRecord();
            originalRecord.Id = 2;
            originalRecord.Name = "Steve";
            originalRecord.Age = 50;

            _db.Users.Update(newRecord, originalRecord);
            GeneratedSqlIs("update [dbo].[Users] set [Id] = @p1 where [dbo].[Users].[Id] = @p2");
            Parameter(0).Is(1);
            Parameter(1).Is(2);
        }

        [Test]
        public void TestUpdateWithDynamicObjectsAndOriginalValues()
        {
            dynamic newRecord = new SimpleRecord();
            newRecord.Id = 1;
            newRecord.Name = "Steve";
            newRecord.Age = 50;
            dynamic originalRecord = new SimpleRecord();
            originalRecord.Id = 2;
            originalRecord.Name = "Steve";
            originalRecord.Age = 50;

            _db.Users.Update(new[] {newRecord}, new[] {originalRecord});
            GeneratedSqlIs("update [dbo].[Users] set [Id] = @p1 where [dbo].[Users].[Id] = @p2");
            Parameter(0).Is(1);
            Parameter(1).Is(2);
        }

        [Test]
        public void TestUpdateWithDynamicObjectList()
        {
            dynamic record1 = new SimpleRecord();
            record1.Id = 1;
            record1.Name = "Steve";
            record1.Age = 50;
            dynamic record2 = new SimpleRecord();
            record2.Id = 2;
            record2.Name = "Bob";
            record2.Age = 42;
            _db.Users.Update(new[] { record1, record2 });
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3");
            Parameter(0).Is("Bob");
            Parameter(1).Is(42);
            Parameter(2).Is(2);
        }

        [Test]
        public void TestUpdateByWithDynamicObjectList()
        {
            dynamic record1 = new SimpleRecord();
            record1.Id = 1;
            record1.Name = "Steve";
            record1.Age = 50;
            dynamic record2 = new SimpleRecord();
            record2.Id = 2;
            record2.Name = "Bob";
            record2.Age = 42;
            _db.Users.UpdateById(new[] { record1, record2 });
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3");
            Parameter(0).Is("Bob");
            Parameter(1).Is(42);
            Parameter(2).Is(2);
        }

        [Test]
        public void TestUpdateByWithDynamicObject()
        {
            dynamic record = new SimpleRecord();
            record.Id = 1;
            record.Name = "Steve";
            record.Age = 50;
            _db.Users.UpdateById(record);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1, [Age] = @p2 where [dbo].[Users].[Id] = @p3");
            Parameter(0).Is("Steve");
            Parameter(1).Is(50);
            Parameter(2).Is(1);
        }

        [Test]
        public void TestUpdateWithStaticObject()
        {
            var user = new User
                           {
                               Id = 1,
                               Name = "Steve",
                               Age = 50
                           };
            _db.Users.Update(user);
            GeneratedSqlIs(
                "update [dbo].[Users] set [Name] = @p1, [Password] = @p2, [Age] = @p3 where [dbo].[Users].[Id] = @p4");
            Parameter(0).Is("Steve");
            Parameter(1).IsDBNull();
            Parameter(2).Is(50);
            Parameter(3).Is(1);
        }

        [Test]
        public void TestUpdateWithStaticObjectAndOriginalObject()
        {
            var newUser = new User
                           {
                               Id = 1,
                               Name = "Steve",
                               Age = 50
                           };
            var originalUser = new User {Id = 2, Name = "Steve", Age = 50};
            _db.Users.Update(newUser, originalUser);
            GeneratedSqlIs(
                "update [dbo].[Users] set [Id] = @p1 where [dbo].[Users].[Id] = @p2");
            Parameter(0).Is(1);
            Parameter(1).Is(2);
        }

        [Test]
        public void TestUpdateWithStaticObjectsAndOriginalObject()
        {
            var newUser = new User
            {
                Id = 1,
                Name = "Steve",
                Age = 50
            };
            var originalUser = new User { Id = 2, Name = "Steve", Age = 50 };
            _db.Users.Update(new[] {newUser}, new[] {originalUser});
            GeneratedSqlIs(
                "update [dbo].[Users] set [Id] = @p1 where [dbo].[Users].[Id] = @p2");
            Parameter(0).Is(1);
            Parameter(1).Is(2);
        }
        
        [Test]
        public void TestUpdateWithStaticObjectWithShoutyCase()
        {
            var user = new User
            {
                Id = 1,
                Name = "Steve",
                Age = 50
            };
            _db.UserTable.Update(user);
            GeneratedSqlIs(
                "update [dbo].[USER_TABLE] set [NAME] = @p1, [PASSWORD] = @p2, [AGE] = @p3 where [dbo].[USER_TABLE].[ID] = @p4");
            Parameter(0).Is("Steve");
            Parameter(1).IsDBNull();
            Parameter(2).Is(50);
            Parameter(3).Is(1);
        }

        [Test]
        public void TestUpdateWithStaticObjectWithAdditionalProperty()
        {
            var user = new RogueUser
            {
                Id = 1,
                Name = "Steve",
                Age = 50,
                RogueProperty = 42
            };
            _db.Users.Update(user);
            GeneratedSqlIs(
                "update [dbo].[Users] set [Name] = @p1, [Password] = @p2, [Age] = @p3 where [dbo].[Users].[Id] = @p4");
            Parameter(0).Is("Steve");
            Parameter(1).IsDBNull();
            Parameter(2).Is(50);
            Parameter(3).Is(1);
        }

        [Test]
        public void TestUpdateWithStaticObjectList()
        {
            var users = new[]
                            {
                                new User { Id = 2, Name = "Bob", Age = 42 },
                                new User { Id = 1, Name = "Steve", Age = 50 }
                            };
            _db.Users.Update(users);
            GeneratedSqlIs(
                "update [dbo].[Users] set [Name] = @p1, [Password] = @p2, [Age] = @p3 where [dbo].[Users].[Id] = @p4");
            Parameter(0).Is("Steve");
            Parameter(1).IsDBNull();
            Parameter(2).Is(50);
            Parameter(3).Is(1);
        }

        [Test]
        public void TestUpdateByWithStaticObjectList()
        {
            var users = new[]
                            {
                                new User { Id = 2, Name = "Bob", Age = 42 },
                                new User { Id = 1, Name = "Steve", Age = 50 }
                            };
            _db.Users.UpdateById(users);
            GeneratedSqlIs(
                "update [dbo].[Users] set [Name] = @p1, [Password] = @p2, [Age] = @p3 where [dbo].[Users].[Id] = @p4");
            Parameter(0).Is("Steve");
            Parameter(1).IsDBNull();
            Parameter(2).Is(50);
            Parameter(3).Is(1);
        }

        [Test]
        public void TestUpdateByWithStaticObject()
        {
            var user = new User
                           {
                               Id = 1,
                               Name = "Steve",
                               Age = 50
                           };
            _db.Users.UpdateById(user);
            GeneratedSqlIs(
                "update [dbo].[Users] set [Name] = @p1, [Password] = @p2, [Age] = @p3 where [dbo].[Users].[Id] = @p4");
            Parameter(0).Is("Steve");
            Parameter(1).IsDBNull();
            Parameter(2).Is(50);
            Parameter(3).Is(1);
        }

        [Test]
        public void TestThatUpdateUsesDbNullForNullValues()
        {
            _db.Users.UpdateById(Id: 1, Name: null);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1 where [dbo].[Users].[Id] = @p2");
            Parameter(0).IsDBNull();
        }

        [Test]
        public void TestUpdateAll()
        {
            _db.Users.UpdateAll(Name: "Steve");
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1");
            Parameter(0).Is("Steve");
        }

        [Test]
        public void TestUpdateWithCriteria()
        {
            _db.Users.UpdateAll(_db.Users.Age > 30, Name: "Steve");
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1 where [dbo].[Users].[Age] > @p2");
            Parameter(0).Is("Steve");
            Parameter(1).Is(30);
        }

        [Test]
        public void TestUpdateWithCriteriaWithNaturalJoin()
        {
            var yearAgo = DateTime.Today.Subtract(TimeSpan.FromDays(365));
            _db.Users.UpdateAll(_db.Users.UserHistory.LastSeen < yearAgo, Name: "Dead User");
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1 where [dbo].[Users].[Id] in " +
                "(select [dbo].[Users].[Id] from [dbo].[Users] join [dbo].[UserHistory] on ([dbo].[Users].[Id] = [dbo].[UserHistory].[UserId]) where [dbo].[UserHistory].[LastSeen] < @p2)");
            Parameter(0).Is("Dead User");
            Parameter(1).Is(yearAgo);
        }

        [Test]
        public void TestUpdateWithCriteriaWithNaturalJoinOnCompoundKeyTable()
        {
            _db.AnnoyingMaster.UpdateAll(_db.AnnoyingMaster.AnnoyingDetail.Value < 42, Text: "Really annoying");
            GeneratedSqlIs("update [dbo].[AnnoyingMaster] set [Text] = @p1 where exists " +
                "(select 1 from [dbo].[AnnoyingMaster] [_updatejoin] join [dbo].[AnnoyingDetail] on ([_updatejoin].[Id1] = [dbo].[AnnoyingDetail].[MasterId1] and [_updatejoin].[Id2] = [dbo].[AnnoyingDetail].[MasterId2]) "+
                "where [dbo].[AnnoyingDetail].[Value] < @p2 and ([_updatejoin].[Id1] = [dbo].[AnnoyingMaster].[Id1] and [_updatejoin].[Id2] = [dbo].[AnnoyingMaster].[Id2]))");
            Parameter(0).Is("Really annoying");
            Parameter(1).Is(42);
        }

        [Test]
        public void TestUpdateWithCriteriaAndDictionary()
        {
            var data = new Dictionary<string, object> { { "Name", "Steve" } };
            _db.Users.UpdateAll(_db.Users.Age > 30, data);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1 where [dbo].[Users].[Age] > @p2");
            Parameter(0).Is("Steve");
            Parameter(1).Is(30);
        }

        [Test]
        public void TestUpdateWithCriteriaAsNamedArg()
        {
            _db.Users.UpdateAll(Name: "Steve", Condition: _db.Users.Age > 30);
            GeneratedSqlIs("update [dbo].[Users] set [Name] = @p1 where [dbo].[Users].[Age] > @p2");
            Parameter(0).Is("Steve");
            Parameter(1).Is(30);
        }
    }
}
