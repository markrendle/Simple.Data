using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
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
            var expected = new[]{ "select top 5 a,b,c from d where a = 1 order by c"};

            var pagedSql = new SqlQueryPager().ApplyLimit(sql, 5);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }

        [Test]
        public void ShouldApplyPagingUsingOrderBy()
        {
            var sql = "select a,b,c from d where a = 1 order by c";
            var expected = new[]{
                "with __data as (select a,b,c, row_number() over(order by c) as [_#_] from d where a = 1)"
                + " select a,b,c from __data where [_#_] between 6 and 15"};

            var pagedSql = new SqlQueryPager().ApplyPaging(sql, 5, 10);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }

        [Test]
        public void ShouldApplyPagingUsingOrderByFirstColumnIfNotAlreadyOrdered()
        {
            var sql = "select a,b,c from d where a = 1";
            var expected = new[]{
                "with __data as (select a,b,c, row_number() over(order by a) as [_#_] from d where a = 1)"
                + " select a,b,c from __data where [_#_] between 11 and 30"};

            var pagedSql = new SqlQueryPager().ApplyPaging(sql, 10, 20);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }

        [Test]
        public void ShouldCopeWithAliasedColumns()
        {
            var sql = "select [a],[b] as [foo],[c] from [d] where [a] = 1";
            var expected =new[]{
                "with __data as (select [a],[b] as [foo],[c], row_number() over(order by [a]) as [_#_] from [d] where [a] = 1)"
                + " select [a],[foo],[c] from __data where [_#_] between 21 and 25"};

            var pagedSql = new SqlQueryPager().ApplyPaging(sql, 20, 5);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }

        [Test]
        public void ShouldCopeWithAliasedDefaultSortColumn()
        {
            var sql = "select [a] as [foo],[b],[c] from [d] where [a] = 1";
            var expected = new[]{
                "with __data as (select [a] as [foo],[b],[c], row_number() over(order by [a]) as [_#_] from [d] where [a] = 1)"
                + " select [foo],[b],[c] from __data where [_#_] between 31 and 40"};

            var pagedSql = new SqlQueryPager().ApplyPaging(sql, 30, 10);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }
    }
}
