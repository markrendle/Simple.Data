using System.Collections.Generic;

namespace Simple.Data.Ado
{
    public interface ICustomQueryBuilder
    {
        ICommandBuilder Build(AdoAdapter adapter, int bulkIndex, SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses);
    }
}
