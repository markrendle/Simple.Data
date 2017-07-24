using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.IntegrationTest.Query
{
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class FunctionTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" });

            schemaProvider.SetColumns(new[] { "dbo", "Users", "Name" },
                                      new[] { "dbo", "Users", "Password" });
        }

        private const string usersColumns = "[dbo].[Users].[Name], [dbo].[Users].[Password]";

        [Test]
        public void SubstringIsEnteredCorrectlyInFindAll()
        {
            const string expected = @"select [dbo].[users].[name],[dbo].[users].[password] from [dbo].[users] where substring([dbo].[users].[name],@p1,@p2) = @p3";

            EatException<InvalidOperationException>(() =>
                _db.Users.FindAll(_db.Users.Name.Substring(0, 1) == "A").ToList());

            GeneratedSqlIs(expected);
            Parameter(0).Is(0);
            Parameter(1).Is(1);
            Parameter(2).Is("A");
        }

        [Test]
        public void SubstringIsEnteredCorrectlyInFindOne()
        {
            const string expected = @"select " + usersColumns + " from [dbo].[users] where substring([dbo].[users].[name],@p1,@p2) = @p3";

            _db.Users.Find(_db.Users.Name.Substring(0, 1) == "A");

            GeneratedSqlIs(expected);
            Parameter(0).Is(0);
            Parameter(1).Is(1);
            Parameter(2).Is("A");
        }

        [Test]
        public void GroupingAndOrderingOnFunction()
        {
            const string expected =
                @"select substring([dbo].[users].[name],@p1,@p2) as [foo],max(substring([dbo].[users].[name],@p3,@p4)) as [bar] from [dbo].[users] group by substring([dbo].[users].[name],@p5,@p6) order by bar desc";

            var column1 = _db.Users.Name.Substring(0, 5).As("Foo");
            var column2 = _db.Users.Name.Substring(5, 5).Max().As("Bar");
            EatException<InvalidOperationException>( () => _db.Users.All()
                                                               .Select(column1, column2)
                                                               .OrderByBarDescending()
                                                               .ToList());

            GeneratedSqlIs(expected);
        }

        [Test]
        public void CountingOnColumn()
        {
            const string expected =
                @"select [dbo].[users].[name],count(distinct isnull([dbo].[users].[password],@p1)) as [c] from [dbo].[users] group by [dbo].[users].[name]";
            EatException<InvalidOperationException>(
                () => _db.Users.All().Select(_db.Users.Name, _db.Users.Password.IsNull("No Password").CountDistinct().As("c")).ToList());
            GeneratedSqlIs(expected);
        }
    }
}
