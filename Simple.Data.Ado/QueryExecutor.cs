namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using Operations;

    internal class QueryExecutor
    {
        public static QueryResult ExecuteQuery(QueryOperation query, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            var queryRunner = new AdoAdapterQueryRunner(adapter, transaction);
            IEnumerable<SimpleQueryClauseBase> unhandled;
            var data = queryRunner.RunQuery(query.Query, out unhandled);
            return new QueryResult(data, unhandled);
        }
    }
}