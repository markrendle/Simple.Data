using System.Collections.Generic;

namespace Simple.Data.Ado
{
    public class QueryBuilder : QueryBuilderBase
    {

        private List<SimpleQueryClauseBase> _unhandledClauses;

        public QueryBuilder(AdoAdapter adoAdapter)
            : base(adoAdapter)
        {
        }

        public QueryBuilder(AdoAdapter adoAdapter, int bulkIndex) : base(adoAdapter, bulkIndex)
        {
        }

        public override ICommandBuilder Build(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {

            var customBuilder = _adoAdapter.ProviderHelper.GetCustomProvider<ICustomQueryBuilder>(_adoAdapter.ConnectionProvider);
            if (customBuilder != null)
            {
                return customBuilder.Build(_adoAdapter, _bulkIndex, query, out unhandledClauses);
            }

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

    }
}