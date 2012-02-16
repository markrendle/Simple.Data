using System.Collections.Generic;

namespace Simple.Data.Ado
{
    public interface IQueryPager
    {
        IEnumerable<string> ApplyLimit(string sql, int take);
        IEnumerable<string> ApplyPaging(string sql, int skip, int take);
    }
}