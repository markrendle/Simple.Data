namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Schema;

    public interface IBulkInserter
    {
        Task<IEnumerable<IDictionary<string, object>>> Insert(AdoAdapter adapter, string tableName, IEnumerable<IReadOnlyDictionary<string, object>> data, IDbTransaction transaction, ErrorCallback onError, bool resultRequired);
    }
}