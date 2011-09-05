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
        public void ShouldApplyPagingUsingOrderBy()
        {
            const string sql = "select a,b,c from d where a = 1 order by c";
            const string expected =
                "with __data as (select a,b,c, row_number() over(order by c) as [_#_] from d where a = 1)"
                + " select a,b,c from __data where [_#_] between @skip + 1 and @skip + @take";

            var modified = new SqlQueryPager().ApplyPaging(sql, "@skip", "@take");
            modified = Normalize.Replace(modified, " ").ToLowerInvariant();

            Assert.AreEqual(expected, modified);
        }

        [Test]
        public void ShouldApplyPagingUsingOrderByFirstColumnIfNotAlreadyOrdered()
        {
            const string sql = "select a,b,c from d where a = 1";
            const string expected =
                "with __data as (select a,b,c, row_number() over(order by a) as [_#_] from d where a = 1)"
                + " select a,b,c from __data where [_#_] between @skip + 1 and @skip + @take";

            var modified = new SqlQueryPager().ApplyPaging(sql, "@skip", "@take");
            modified = Normalize.Replace(modified, " ").ToLowerInvariant();

            Assert.AreEqual(expected, modified);
        }

        [Test]
        public void ShouldCopeWithAliasedColumns()
        {
            const string sql = "select [a],[b] as [foo],[c] from [d] where [a] = 1";
            const string expected =
                "with __data as (select [a],[b] as [foo],[c], row_number() over(order by [a]) as [_#_] from [d] where [a] = 1)"
                + " select [a],[foo],[c] from __data where [_#_] between @skip + 1 and @skip + @take";

            var modified = new SqlQueryPager().ApplyPaging(sql, "@skip", "@take");
            modified = Normalize.Replace(modified, " ").ToLowerInvariant();

            Assert.AreEqual(expected, modified);
        }
    }
}
