using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Linq;
using Shitty.Data.SqlCe40;

namespace Shitty.Data.SqlCe40Test
{
    [TestFixture]
    public class SqlCe40QueryPagerTest
    {
        static readonly Regex Normalize = new Regex(@"\s+", RegexOptions.Multiline);

        [Test]
        public void ShouldApplyLimitUsingTop()
        {
            var sql = "select a,b,c from d where a = 1 order by c";
            var expected = new[] { "select top(5) a,b,c from d where a = 1 order by c" };

            var pagedSql = new SqlCe40QueryPager().ApplyLimit(sql, 5);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }

        [Test]
        public void ShouldApplyLimitUsingTopWithDistinct()
        {
            var sql = "select distinct a,b,c from d where a = 1 order by c";
            var expected = new[] { "select distinct top(5) a,b,c from d where a = 1 order by c" };

            var pagedSql = new SqlCe40QueryPager().ApplyLimit(sql, 5);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }

        [Test]
        public void ShouldApplyPagingUsingOrderBy()
        {
            var sql = "select a,b,c from d where a = 1 order by c";
            var expected = new[]{
                "select a,b,c from d where a = 1 order by c offset 5 rows fetch next 10 rows only"};

            var pagedSql = new SqlCe40QueryPager().ApplyPaging(sql, new string[0], 5, 10);
            var modified = pagedSql.Select(x=> Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }

        [Test]
        public void ShouldApplyPagingUsingOrderByFirstColumnIfNotAlreadyOrdered()
        {
            var sql = "select a,b,c from d where a = 1";
            var expected = new[]{
                "select a,b,c from d where a = 1 order by a offset 10 rows fetch next 20 rows only"};

            var pagedSql = new SqlCe40QueryPager().ApplyPaging(sql, new string[0], 10, 20);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }
    }
}
