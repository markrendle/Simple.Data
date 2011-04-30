using System.Text.RegularExpressions;
using NUnit.Framework;
using Simple.Data.SqlCe40;

namespace Simple.Data.SqlCe40Test
{
    [TestFixture]
    public class SqlCe40QueryPagerTest
    {
        static readonly Regex Normalize = new Regex(@"\s+", RegexOptions.Multiline);

        [Test]
        public void ShouldApplyPagingUsingOrderBy()
        {
            const string sql = "select a,b,c from d where a = 1 order by c";
            const string expected =
                "select a,b,c from d where a = 1 order by c offset @skip rows fetch next @take rows only";

            var modified = new SqlCe40QueryPager().ApplyPaging(sql, "@skip", "@take");
            modified = Normalize.Replace(modified, " ").ToLowerInvariant();

            Assert.AreEqual(expected, modified);
        }

        [Test]
        public void ShouldApplyPagingUsingOrderByFirstColumnIfNotAlreadyOrdered()
        {
            const string sql = "select a,b,c from d where a = 1";
            const string expected =
                "select a,b,c from d where a = 1 order by a offset @skip rows fetch next @take rows only";

            var modified = new SqlCe40QueryPager().ApplyPaging(sql, "@skip", "@take");
            modified = Normalize.Replace(modified, " ").ToLowerInvariant();

            Assert.AreEqual(expected, modified);
        }
    }
}
