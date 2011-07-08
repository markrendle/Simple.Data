namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using System.Data;

    public interface IBulkUpdater
    {
        int Update(AdoAdapter adapter, string tableName, IList<IDictionary<string, object>> data, IDbTransaction transaction);
    }
}