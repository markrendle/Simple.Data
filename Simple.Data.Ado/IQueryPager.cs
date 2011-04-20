namespace Simple.Data.Ado
{
    public interface IQueryPager
    {
        string ApplyPaging(string sql, string skipParameterName, string takeParameterName);
    }
}