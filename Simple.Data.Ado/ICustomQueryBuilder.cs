using System.Collections.Generic;

namespace Shitty.Data.Ado
{
    public interface ICustomQueryBuilder
    {
        ICommandBuilder Build(AdoAdapter adapter, int bulkIndex, SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses);
    }
}
