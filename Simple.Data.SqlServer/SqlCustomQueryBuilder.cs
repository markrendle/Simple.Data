using System.Collections.Generic;
using System.ComponentModel.Composition;
using Shitty.Data.Ado;

namespace Shitty.Data.SqlServer
{

    [Export(typeof(ICustomQueryBuilder))]
    public class SqlCustomQueryBuilder : ICustomQueryBuilder
    {
        public ICommandBuilder Build(AdoAdapter adapter, int bulkIndex, SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return new SqlQueryBuilder(adapter, bulkIndex).Build(query, out unhandledClauses);
        }
    }
}
