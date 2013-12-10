using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Simple.Data.Ado;
using Simple.Data.SqlServer;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class SqlQueryPagerTest
    {
        static readonly Regex Normalize = new Regex(@"\s+", RegexOptions.Multiline);

        [Test]
        public void ShouldApplyLimitUsingTop()
        {
            var sql = "select a,b,c from d where a = 1 order by c";
            var expected = new[] { "select top 5 a,b,c from d where a = 1 order by c" };

            var pagedSql = new SqlQueryPager().ApplyLimit(sql, 5);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }

        [Test]
        public void ShouldApplyLimitUsingTopWithDistinct()
        {
            var sql = "select distinct a,b,c from d where a = 1 order by c";
            var expected = new[] { "select distinct top 5 a,b,c from d where a = 1 order by c" };

            var pagedSql = new SqlQueryPager().ApplyLimit(sql, 5);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }

        [Test]
        public void ShouldApplyPagingUsingOrderBy()
        {
            var sql = "select [dbo].[d].[a],[dbo].[d].[b],[dbo].[d].[c] from [dbo].[d] where [dbo].[d].[a] = 1 order by [dbo].[d].[c]";
            var expected = new[]{
                "with __data as (select [dbo].[d].[a], row_number() over(order by [dbo].[d].[c]) as [_#_] from [dbo].[d] where [dbo].[d].[a] = 1)"
                + " select [dbo].[d].[a],[dbo].[d].[b],[dbo].[d].[c] from __data join [dbo].[d] on [dbo].[d].[a] = __data.[a] where [dbo].[d].[a] = 1 and [_#_] between 6 and 15"};

            var pagedSql = new SqlQueryPager().ApplyPaging(sql, new[] {"[dbo].[d].[a]"}, 5, 10);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant()).ToArray();

            Assert.AreEqual(expected[0], modified[0]);
        }

        [Test]
        public void ShouldApplyPagingUsingOrderByKeysIfNotAlreadyOrdered()
        {
            var sql = "select [dbo].[d].[a],[dbo].[d].[b],[dbo].[d].[c] from [dbo].[d] where [dbo].[d].[a] = 1";
            var expected = new[]{
                "with __data as (select [dbo].[d].[a], row_number() over(order by [dbo].[d].[a]) as [_#_] from [dbo].[d] where [dbo].[d].[a] = 1)"
                + " select [dbo].[d].[a],[dbo].[d].[b],[dbo].[d].[c] from __data join [dbo].[d] on [dbo].[d].[a] = __data.[a] where [dbo].[d].[a] = 1 and [_#_] between 11 and 30"};

            var pagedSql = new SqlQueryPager().ApplyPaging(sql, new[] {"[dbo].[d].[a]"}, 10, 20);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant()).ToArray();

            Assert.AreEqual(expected[0], modified[0]);
        }

        [Test]
        public void ShouldCopeWithAliasedColumns()
        {
            var sql = "select [dbo].[d].[a],[dbo].[d].[b] as [foo],[dbo].[d].[c] from [dbo].[d] where [dbo].[d].[a] = 1";
            var expected =new[]{
                "with __data as (select [dbo].[d].[a], row_number() over(order by [dbo].[d].[a]) as [_#_] from [dbo].[d] where [dbo].[d].[a] = 1)"
                + " select [dbo].[d].[a],[dbo].[d].[b] as [foo],[dbo].[d].[c] from __data join [dbo].[d] on [dbo].[d].[a] = __data.[a] where [dbo].[d].[a] = 1 and [_#_] between 21 and 25"};

            var pagedSql = new SqlQueryPager().ApplyPaging(sql, new[]{"[dbo].[d].[a]"}, 20, 5);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant()).ToArray();

            Assert.AreEqual(expected[0], modified[0]);
        }

        [Test]
        public void ShouldCopeWithColumnsThatEndInFrom()
        {
            const string sql = @"SELECT [dbo].[PromoPosts].[Id],[dbo].[PromoPosts].[ActiveFrom],[dbo].[PromoPosts].[ActiveTo],[dbo].[PromoPosts].[Created],[dbo].[PromoPosts].[Updated] 
    from [dbo].[PromoPosts] 
    ORDER BY [dbo].[PromoPosts].[ActiveFrom]";

            var expected = @"with __data as (select [dbo].[promoposts].[id], row_number() over(order by [dbo].[promoposts].[activefrom]) as [_#_] from [dbo].[promoposts]) select [dbo].[promoposts].[id],[dbo].[promoposts].[activefrom],[dbo].[promoposts].[activeto],[dbo].[promoposts].[created],[dbo].[promoposts].[updated] from __data join [dbo].[promoposts] on [dbo].[promoposts].[id] = __data.[id] and [_#_] between 1 and 25";
            expected = expected.ToLowerInvariant();

            var pagedSql = new SqlQueryPager().ApplyPaging(sql, new[] {"[dbo].[PromoPosts].[Id]"}, 0, 25).Single();
            var modified = Normalize.Replace(pagedSql, " ").ToLowerInvariant();
            Assert.AreEqual(expected, modified);
        }

        [Test]
        public void ShouldExcludeLeftJoinedTablesFromSubSelect()
        {
            const string sql = @"SELECT [dbo].[MainClass].[ID],
    [dbo].[MainClass].[SomeProperty],
    [dbo].[MainClass].[SomeProperty2],
    [dbo].[MainClass].[SomeProperty3],
    [dbo].[MainClass].[SomeProperty4],
    [dbo].[ChildClass].[ID] AS [__withn__ChildClass__ID],
    [dbo].[ChildClass].[SomeProperty] AS [__withn__ChildClass__SomeProperty],
    [dbo].[ChildClass].[SomeProperty2] AS [__withn__ChildClass__SomeProperty2] FROM [dbo].[MainClass] LEFT JOIN [dbo].[JoinTable] ON ([dbo].[MainClass].[ID] = [dbo].[JoinTable].[MainClassID]) LEFT JOIN [dbo].[ChildClass] ON ([dbo].[ChildClass].[ID] = [dbo].[JoinTable].[ChildClassID]) WHERE ([dbo].[MainClass].[SomeProperty] > @p1 AND [dbo].[MainClass].[SomeProperty] <= @p2)";

            const string expected = @"with __data as (select [dbo].[promoposts].[id], row_number() over(order by [dbo].[promoposts].[id]) as [_#_] from [dbo].[mainclass] where ([dbo].[mainclass].[someproperty] > @p1 and [dbo].[mainclass].[someproperty] <= @p2)) select [dbo].[mainclass].[id], [dbo].[mainclass].[someproperty], [dbo].[mainclass].[someproperty2], [dbo].[mainclass].[someproperty3], [dbo].[mainclass].[someproperty4], [dbo].[childclass].[id] as [__withn__childclass__id], [dbo].[childclass].[someproperty] as [__withn__childclass__someproperty], [dbo].[childclass].[someproperty2] as [__withn__childclass__someproperty2] from __data join [dbo].[promoposts] on [dbo].[promoposts].[id] = __data.[id]from [dbo].[mainclass] left join [dbo].[jointable] on ([dbo].[mainclass].[id] = [dbo].[jointable].[mainclassid]) left join [dbo].[childclass] on ([dbo].[childclass].[id] = [dbo].[jointable].[childclassid]) where ([dbo].[mainclass].[someproperty] > @p1 and [dbo].[mainclass].[someproperty] <= @p2) and [_#_] between 1 and 25";

            var pagedSql = new SqlQueryPager().ApplyPaging(sql, new[] {"[dbo].[PromoPosts].[Id]"}, 0, 25).Single();
            var modified = Normalize.Replace(pagedSql, " ").ToLowerInvariant();
            Assert.AreEqual(expected, modified);
        }

        [Test]
        public void ShouldThrowIfTableHasNoPrimaryKey([Values(null, new string[0])]string[] keys)
        {
            var sql = "select [dbo].[d].[a] from [dbo].[b]";

            Assert.Throws<AdoAdapterException>(
                () => new SqlQueryPager().ApplyPaging(sql, keys, 5, 10).ToList()
            );
        }
    }
}
