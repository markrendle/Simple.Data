namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using System.Data;

    public interface IBulkUpdater
    {
        int Update(AdoAdapter adapter, string tableName, IEnumerable<IReadOnlyDictionary<string, object>> data, IDbTransaction transaction);
        int Update(AdoAdapter adapter, string tableName, IEnumerable<IReadOnlyDictionary<string, object>> toList, IEnumerable<string> criteriaFieldNames, IDbTransaction dbTransaction);
    }
}