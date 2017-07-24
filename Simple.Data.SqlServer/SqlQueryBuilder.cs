using System.Collections.Generic;
using System.Linq;
using Shitty.Data.Ado;

namespace Shitty.Data.SqlServer
{
    public class SqlQueryBuilder : QueryBuilderBase
    {
        private List<SimpleQueryClauseBase> _unhandledClauses;

        public SqlQueryBuilder(AdoAdapter adapter, int bulkIndex)
            : base(adapter, bulkIndex)
        {
        }

        public override ICommandBuilder Build(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            _unhandledClauses = new List<SimpleQueryClauseBase>();
            SetQueryContext(query);

            HandleJoins();
            HandleQueryCriteria();
            HandleGrouping();
            HandleHavingCriteria();
            HandleOrderBy();

            unhandledClauses = _unhandledClauses;
            return _commandBuilder;
        }

        protected override string GetSelectClause(ObjectName tableName)
        {
            var select = base.GetSelectClause(tableName);
            var forUpdateClause = _query.Clauses.OfType<ForUpdateClause>().FirstOrDefault();
            if (forUpdateClause != null)
            {
                var forUpdate = " WITH (UPDLOCK, ROWLOCK";
                if (forUpdateClause.SkipLockedRows)
                {
                    forUpdate += ", READPAST";
                }
                forUpdate += ")";
                select += forUpdate;
            }
            return select;
        }
    }
}
