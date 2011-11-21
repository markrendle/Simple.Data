using System.Text.RegularExpressions;
using NUnit.Framework;
using Simple.Data.SqlCe40;
using System.Linq;

namespace Simple.Data.SqlCe40Test
{
    [TestFixture]
    public class SqlCe40QueryPagerTest
    {
        static readonly Regex Normalize = new Regex(@"\s+", RegexOptions.Multiline);

        [Test]
        public void ShouldApplyPagingUsingOrderBy()
        {
            var sql = "select a,b,c from d where a = 1 order by c";
            var expected = new[]{
                "select a,b,c from d where a = 1 order by c offset 5 rows fetch next 10 rows only"};

            var pagedSql = new SqlCe40QueryPager().ApplyPaging(sql, 5, 10);
            var modified = pagedSql.Select(x=> Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }

        [Test]
        public void ShouldApplyPagingUsingOrderByFirstColumnIfNotAlreadyOrdered()
        {
            var sql = "select a,b,c from d where a = 1";
            var expected = new[]{
                "select a,b,c from d where a = 1 order by a offset 10 rows fetch next 20 rows only"};

            var pagedSql = new SqlCe40QueryPager().ApplyPaging(sql, 10, 20);
            var modified = pagedSql.Select(x => Normalize.Replace(x, " ").ToLowerInvariant());

            Assert.IsTrue(expected.SequenceEqual(modified));
        }
    }
}
