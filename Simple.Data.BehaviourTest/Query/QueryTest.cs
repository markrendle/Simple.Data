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
                                     new[] {"dbo", "UserBio", "BASE TABLE"},
                                     new[] {"dbo", "Employee", "BASE TABLE"});

            schemaProvider.SetColumns(new object[] {"dbo", "Users", "Id", true},
                                      new[] {"dbo", "Users", "Name"},
                                      new[] {"dbo", "Users", "Password"},
                                      new[] {"dbo", "Users", "Age"},
                                      new[] {"dbo", "UserBio", "UserId"},
                                      new[] {"dbo", "UserBio", "Text"},
                                      new[] {"dbo", "Employee", "Id"},
                                      new[] {"dbo", "Employee", "Name"},
                                      new[] {"dbo", "Employee", "ManagerId"});

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
        public void SpecifyingColumnWithAliasShouldAddAsClause()
        {
            _db.Users.All()
                .Select(_db.Users.Name, _db.Users.Password.As("SuperSecretPassword"))
                .ToList();
            GeneratedSqlIs("select [dbo].[users].[name],[dbo].[users].[password] as [supersecretpassword] from [dbo].[users]");
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
            catch (SimpleDataException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select count(*) from [dbo].[users]");
        }

        [Test]
        public void SpecifyingExistsShouldSelectDistinct1()
        {
            try
            {
                _db.Users.All().Exists();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select distinct 1 from [dbo].[users]");
        }

        [Test]
        public void SpecifyingAnyShouldSelectDistinct1()
        {
            try
            {
                _db.Users.All().Any();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select distinct 1 from [dbo].[users]");
        }

        [Test]
        public void SpecifyingMinShouldSelectFunction()
        {
            try
            {
                _db.Users.All().Select(_db.Users.Age.Min()).ToScalar();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }
            catch (SimpleDataException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select min([dbo].[users].[age]) from [dbo].[users]");
        }

        [Test]
        public void SpecifyingNonAggregatedColumnAndMinShouldAddGroupBy()
        {
            try
            {
                _db.Users.All().Select(_db.Users.Name, _db.Users.Age.Min().As("Youngest")).ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select [dbo].[users].[name],min([dbo].[users].[age]) as [youngest] from [dbo].[users] group by [dbo].[users].[name]");
        }

        [Test]
        public void SpecifyingNonAggregateFunctionAndMinShouldAddGroupBy()
        {
            try
            {
                _db.Users.All().Select(_db.Users.Name.Length(), _db.Users.Age.Min().As("Youngest")).ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select len([dbo].[users].[name]),min([dbo].[users].[age]) as [youngest] from [dbo].[users] group by [dbo].[users].[name]");
        }

        [Test]
        public void SpecifyingNonAggregateFunctionShouldNotApplyGroupBy()
        {
            try
            {
                _db.Users.All().Select(_db.Users.Name, _db.Users.Name.Length().As("NameLength")).ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }
            GeneratedSqlIs("select [dbo].[users].[name],len([dbo].[users].[name]) as [namelength] from [dbo].[users]");
        }

        [Test]
        public void SpecifyingJoinTableShouldCreateDirectQuery()
        {
            try
            {
                _db.Users.QueryById(1).UserBio.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }
            GeneratedSqlIs("select [dbo].[userbio].[userid],[dbo].[userbio].[text] from [dbo].[userbio]" +
                " join [dbo].[users] on ([dbo].[users].[id] = [dbo].[userbio].[userid]) where [dbo].[users].[id] = @p1");
        }

        
    }
}
