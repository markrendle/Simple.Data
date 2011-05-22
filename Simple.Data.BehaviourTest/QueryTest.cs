using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestFixture]
    public class QueryTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] {"dbo", "Users", "BASE TABLE"},
                                     new[] {"dbo", "UserBio", "BASE TABLE"});

            schemaProvider.SetColumns(new object[] {"dbo", "Users", "Id", true},
                                      new[] {"dbo", "Users", "Name"},
                                      new[] {"dbo", "Users", "Password"},
                                      new[] {"dbo", "Users", "Age"},
                                      new[] {"dbo", "UserBio", "UserId"},
                                      new[] {"dbo", "UserBio", "Text"});

            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Users", "Id", 0 });
            schemaProvider.SetForeignKeys(new object[] { "FK_Users_UserBio", "dbo", "UserBio", "UserId", "dbo", "Users", "Id", 0 });
        }

        [Test]
        public void SpecifyingColumnsShouldRestrictSelect()
        {
            _db.Users.All()
                .Select(_db.Users.Name, _db.Users.Password)
                .ToList();
            GeneratedSqlIs("select [dbo].[users].[name],[dbo].[users].[password] from [dbo].[users]");
        }

        [Test]
        public void SpecifyingColumnsFromOtherTablesShouldAddJoin()
        {
            _db.Users.All()
                .Select(_db.Users.Name, _db.Users.Password, _db.Users.UserBio.Text)
                .ToList();
            GeneratedSqlIs(
                "select [dbo].[users].[name],[dbo].[users].[password],[dbo].[userbio].[text] from [dbo].[users]" +
                " join [dbo].[userbio] on ([dbo].[users].[id] = [dbo].[userbio].[userid])");
        }

        [Test]
        public void SpecifyingCountShouldSelectCount()
        {
            try
            {
                _db.Users.All().Count();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select count(*) from [dbo].[users]");
        }

        [Test]
        public void SpecifyingExistsShouldSelectCount()
        {
            try
            {
                _db.Users.All().Exists();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select count(*) from [dbo].[users]");
        }

        [Test]
        public void SpecifyingAnyShouldSelectCount()
        {
            try
            {
                _db.Users.All().Any();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select count(*) from [dbo].[users]");
        }
    }
}
