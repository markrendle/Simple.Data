using System.Collections.Generic;

namespace Simple.Data.Ado
{
    public interface IQueryPager
    {
        IEnumerable<string> ApplyPaging(string sql, int skip, int take);
    }
}