using System.Collections.Generic;

namespace Shitty.Data.Ado
{
    public interface IQueryPager
    {
        IEnumerable<string> ApplyLimit(string sql, int take);
        IEnumerable<string> ApplyPaging(string sql, string[] keys, int skip, int take);
    }
}