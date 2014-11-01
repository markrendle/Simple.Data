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
                                     new[] { "dbo", "UserPayment", "BASE TABLE" },
                                     new[] { "dbo", "Employee", "BASE TABLE" });

            schemaProvider.SetColumns(new object[] {"dbo", "Users", "Id", true},
                                      new[] {"dbo", "Users", "Name"},
                                      new[] {"dbo", "Users", "Password"},
                                      new[] {"dbo", "Users", "Age"},
                                      new[] {"dbo", "UserBio", "UserId"},
                                      new[] {"dbo", "UserBio", "Text"},
                                      new[] { "dbo", "UserPayment", "UserId" },
                                      new[] { "dbo", "UserPayment", "Amount" },
                                      new[] { "dbo", "Employee", "Id" },
                                      new[] {"dbo", "Employee", "Name"},
                                      new[] {"dbo", "Employee", "ManagerId"});

            schemaProvider.SetPrimaryKeys(new object[] { "dbo", "Users", "Id", 0 });
            schemaProvider.SetForeignKeys(new object[] { "FK_Users_UserBio", "dbo", "UserBio", "UserId", "dbo", "Users", "Id", 0 },
                                          new object[] { "FK_Users_UserPayment", "dbo", "UserPayment", "UserId", "dbo", "Users", "Id", 0 });
        }

        [Test]
        public void SpecifyingColumnsShouldRestrictSelect()
        {
            TargetDb.Users.All()
                .Select(TargetDb.Users.Name, TargetDb.Users.Password)
                .ToList();
            GeneratedSqlIs("select [dbo].[users].[name],[dbo].[users].[password] from [dbo].[users]");
        }

        [Test]
        public void SpecifyingColumnStarShouldSelectAllColumns()
        {
            TargetDb.Users.All()
                .Select(TargetDb.Users.Name, TargetDb.Users.UserBio.Star())
                .ToList();
            GeneratedSqlIs("select [dbo].[users].[name],[dbo].[userbio].[userid],[dbo].[userbio].[text] from [dbo].[users]" +
                " left join [dbo].[userbio] on ([dbo].[users].[id] = [dbo].[userbio].[userid])");
        }

        [Test]
        public void SpecifyingColumnAllColumnsShouldSelectAllColumns()
        {
            TargetDb.Users.All()
                .Select(TargetDb.Users.Name, TargetDb.Users.UserBio.AllColumns())
                .ToList();
            GeneratedSqlIs("select [dbo].[users].[name],[dbo].[userbio].[userid],[dbo].[userbio].[text] from [dbo].[users]" +
                " left join [dbo].[userbio] on ([dbo].[users].[id] = [dbo].[userbio].[userid])");
        }

        [Test]
        public void SpecifyingColumnWithAliasShouldAddAsClause()
        {
            TargetDb.Users.All()
                .Select(TargetDb.Users.Name, TargetDb.Users.Password.As("SuperSecretPassword"))
                .ToList();
            GeneratedSqlIs("select [dbo].[users].[name],[dbo].[users].[password] as [supersecretpassword] from [dbo].[users]");
        }
        
        [Test]
        public void SpecifyingColumnMathsWithAliasShouldAddAsClause()
        {
            TargetDb.Users.All()
                .Select(TargetDb.Users.Name, (TargetDb.Users.Id + TargetDb.Users.Age).As("Nonsense"))
                .ToList();
            GeneratedSqlIs("select [dbo].[users].[name],([dbo].[users].[id] + [dbo].[users].[age]) as [nonsense] from [dbo].[users]");
        }

        [Test]
        public void SpecifyingColumnsFromOtherTablesShouldAddJoin()
        {
            TargetDb.Users.All()
                .Select(TargetDb.Users.Name, TargetDb.Users.Password, TargetDb.Users.UserBio.Text)
                .ToList();
            GeneratedSqlIs(
                "select [dbo].[users].[name],[dbo].[users].[password],[dbo].[userbio].[text] from [dbo].[users]" +
                " left join [dbo].[userbio] on ([dbo].[users].[id] = [dbo].[userbio].[userid])");
        }

        [Test]
        public void SpecifyingColumnsAndAggregatesFromOtherTablesShouldAddJoins()
        {
            TargetDb.Users.All()
                .Select(TargetDb.Users.Name, TargetDb.Users.Password, TargetDb.Users.UserBio.Text, TargetDb.Users.UserPayments.Amount.Sum())
                .ToList();
            GeneratedSqlIs(
                "select [dbo].[users].[name],[dbo].[users].[password],[dbo].[userbio].[text],sum([dbo].[userpayment].[amount]) from [dbo].[users]" +
                " left join [dbo].[userbio] on ([dbo].[users].[id] = [dbo].[userbio].[userid])" +
                " left join [dbo].[userpayment] on ([dbo].[users].[id] = [dbo].[userpayment].[userid])" +
                " group by [dbo].[users].[name],[dbo].[users].[password],[dbo].[userbio].[text]"
                );
        }
        
        [Test]
        public void SpecifyingCountShouldSelectCount()
        {
            try
            {
                TargetDb.Users.All().Count();
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
                TargetDb.Users.All().Exists();
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
                TargetDb.Users.All().Any();
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
                TargetDb.Users.All().Select(TargetDb.Users.Age.Min()).ToScalar();
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
                TargetDb.Users.All().Select(TargetDb.Users.Name, TargetDb.Users.Age.Min().As("Youngest")).ToList();
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
                TargetDb.Users.All().Select(TargetDb.Users.Name.Length(), TargetDb.Users.Age.Min().As("Youngest")).ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select len([dbo].[users].[name]),min([dbo].[users].[age]) as [youngest] from [dbo].[users] group by len([dbo].[users].[name])");
        }

        [Test]
        public void SpecifyingNonAggregateFunctionShouldNotApplyGroupBy()
        {
            try
            {
                TargetDb.Users.All().Select(TargetDb.Users.Name, TargetDb.Users.Name.Length().As("NameLength")).ToList();
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
                TargetDb.Users.QueryById(1).UserBio.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }
            GeneratedSqlIs("select [dbo].[userbio].[userid],[dbo].[userbio].[text] from [dbo].[userbio]" +
                " join [dbo].[users] on ([dbo].[users].[id] = [dbo].[userbio].[userid]) where [dbo].[users].[id] = @p1");
        }

        [Test]
        public void SpecifyOrderByWithoutReferenceThrowsException()
        {
            Assert.Throws<ArgumentException>(() => TargetDb.Users.All().OrderBy(1));
        }
        
        [Test]
        public void SpecifyOrderByDescendingWithoutReferenceThrowsException()
        {
            Assert.Throws<ArgumentException>(() => TargetDb.Users.All().OrderByDescending(1));
        }
    }
}
