using System.Collections.Generic;
using System.Linq;
using Simple.Data.Ado;

namespace Simple.Data.SqlServer
{
    public class SqlQueryBuilder : QueryBuilderBase
    {
        private List<SimpleQueryClauseBase> _unhandledClauses;

        public SqlQueryBuilder(AdoAdapter adapter, int bulkIndex)
            : base(adapter, bulkIndex)
        {
        }

        public override ICommandBuilder Build(SimpleQuery query, List<SimpleQueryClauseBase> unhandledClauses)
        {
            _unhandledClauses = unhandledClauses;
            SetQueryContext(query);

            HandleJoins();
            HandleQueryCriteria();
            HandleGrouping();
            HandleHavingCriteria();
            HandleOrderBy();

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
