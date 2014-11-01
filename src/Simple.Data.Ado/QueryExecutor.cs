namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Operations;

    internal class QueryExecutor
    {
        public static async Task<OperationResult> ExecuteQuery(QueryOperation query, AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            var queryRunner = new AdoAdapterQueryRunner(adapter, transaction);
            var unhandled = new List<SimpleQueryClauseBase>();
            var data = await queryRunner.RunQuery(query.Query, unhandled);
            return new QueryResult(data, unhandled);
        }
    }
}