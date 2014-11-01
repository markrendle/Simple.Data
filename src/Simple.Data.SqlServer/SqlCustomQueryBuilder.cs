using System.Collections.Generic;
using System.ComponentModel.Composition;
using Simple.Data.Ado;

namespace Simple.Data.SqlServer
{

    [Export(typeof(ICustomQueryBuilder))]
    public class SqlCustomQueryBuilder : ICustomQueryBuilder
    {
        public ICommandBuilder Build(AdoAdapter adapter, int bulkIndex, SimpleQuery query, List<SimpleQueryClauseBase> unhandledClauses)
        {
            return new SqlQueryBuilder(adapter, bulkIndex).Build(query, unhandledClauses);
        }
    }
}
