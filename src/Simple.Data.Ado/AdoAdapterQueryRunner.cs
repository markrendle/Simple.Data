namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    internal class AdoAdapterQueryRunner
    {
        private readonly AdoAdapter _adapter;
        private readonly AdoAdapterTransaction _transaction;

        public AdoAdapterQueryRunner(AdoAdapter adapter) : this(adapter, null)
        {
        }

        public AdoAdapterQueryRunner(AdoAdapter adapter, AdoAdapterTransaction transaction)
        {
            _adapter = adapter;
            _transaction = transaction;
        }

        public async Task<IEnumerable<IDictionary<string, object>>> RunQuery(SimpleQuery query, List<SimpleQueryClauseBase> unhandledClauses)
        {
            IEnumerable<IDictionary<string, object>> result;

            if (query.Clauses.OfType<WithCountClause>().Any()) return await RunQueryWithCount(query, unhandledClauses);

            ICommandBuilder[] commandBuilders = GetQueryCommandBuilders(ref query, unhandledClauses);
            IDbConnection connection = _adapter.CreateConnection();
            if (_adapter.ProviderSupportsCompoundStatements || commandBuilders.Length == 1)
            {
                var command =
                    new CommandBuilder(_adapter.GetSchema()).CreateCommand(
                        _adapter.ProviderHelper.GetCustomProvider<IDbParameterFactory>(_adapter.SchemaProvider),
                        commandBuilders,
                        connection, _adapter.AdoOptions);

                if (_transaction != null)
                {
                    command.Transaction = _transaction.DbTransaction;
                    result = command.ToEnumerable(_transaction.DbTransaction);
                }
                else
                {
                    result = command.ToEnumerable(_adapter.CreateConnection);
                }
            }
            else
            {
                result = commandBuilders.SelectMany(cb => cb.GetCommand(connection, _adapter.AdoOptions).ToEnumerable(_adapter.CreateConnection));
            }

            if (query.Clauses.OfType<WithClause>().Any())
            {
                result = new EagerLoadingEnumerable(result);
            }

            return result;
        }

        private async Task<IEnumerable<IDictionary<string, object>>> RunQueryWithCount(SimpleQuery query, List<SimpleQueryClauseBase> unhandledClauses)
        {
            WithCountClause withCountClause;
            try
            {
                withCountClause = query.Clauses.OfType<WithCountClause>().First();
            }
            catch (InvalidOperationException e)
            {
                // Rethrow with meaning.
                throw new InvalidOperationException("No WithCountClause specified.", e);
            }

            query = query.ClearWithTotalCount();
            var countQuery = query.ClearSkip()
                .ClearTake()
                .ClearOrderBy()
                .ClearWith()
                .ClearForUpdate()
                .ReplaceSelect(new CountSpecialReference());
            var unhandledClausesList = new List<IEnumerable<SimpleQueryClauseBase>>
                                           {
                                               Enumerable.Empty<SimpleQueryClauseBase>(),
                                               Enumerable.Empty<SimpleQueryClauseBase>()
                                           };

            using (var enumerator = RunQueries(new[] {countQuery, query}, unhandledClausesList).GetEnumerator())
            {
                unhandledClauses.AddRange(unhandledClausesList[1]);
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException();
                }
                var result = await enumerator.Current;
                IDictionary<string, object> countRow = result.Single();
                var value = countRow.First().Value;
                int count = value is int ? (int) value : Convert.ToInt32(value);
                withCountClause.SetCount(count);
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException();
                }
                return await enumerator.Current;
            }
        }

        private ICommandBuilder[] GetPagedQueryCommandBuilders(ref SimpleQuery query, List<SimpleQueryClauseBase> unhandledClauses)
        {
            return GetPagedQueryCommandBuilders(ref query, -1, unhandledClauses);
        }

        private ICommandBuilder[] GetPagedQueryCommandBuilders(ref SimpleQuery query, Int32 bulkIndex,
                                                               List<SimpleQueryClauseBase> unhandledClauses)
        {
            var commandBuilders = new List<ICommandBuilder>();

            IEnumerable<SimpleQueryClauseBase> unhandledClausesForPagedQuery;
            ICommandBuilder mainCommandBuilder = new QueryBuilder(_adapter, bulkIndex).Build(query,
                                                                                             out
                                                                                                 unhandledClausesForPagedQuery);
            unhandledClauses.AddRange(unhandledClausesForPagedQuery);

            SkipClause skipClause = query.Clauses.OfType<SkipClause>().FirstOrDefault();
            TakeClause takeClause = query.Clauses.OfType<TakeClause>().FirstOrDefault();

            if (skipClause != null || takeClause != null)
            {
                var queryPager = _adapter.ProviderHelper.GetCustomProvider<IQueryPager>(_adapter.ConnectionProvider);
                if (queryPager == null)
                {
                    SimpleDataTraceSources.TraceSource.TraceEvent(TraceEventType.Warning, SimpleDataTraceSources.PerformanceWarningMessageId,
                        "There is no database-specific query paging in your current Simple.Data Provider. Paging will be done in memory.");
                    DeferPaging(ref query, mainCommandBuilder, commandBuilders, unhandledClauses);
                }
                else
                {
                    ApplyPaging(query, commandBuilders, mainCommandBuilder, skipClause, takeClause, query.Clauses.OfType<WithClause>().Any(), queryPager);
                }
            }
            return commandBuilders.ToArray();
        }

        private void DeferPaging(ref SimpleQuery query, ICommandBuilder mainCommandBuilder, List<ICommandBuilder> commandBuilders,
                                 List<SimpleQueryClauseBase> unhandledClausesList)
        {
            unhandledClausesList.AddRange(query.Clauses.OfType<SkipClause>());
            unhandledClausesList.AddRange(query.Clauses.OfType<TakeClause>());
            query = query.ClearSkip().ClearTake();
            var commandBuilder = new CommandBuilder(mainCommandBuilder.Text, _adapter.GetSchema(),
                                                    mainCommandBuilder.Parameters);
            commandBuilders.Add(commandBuilder);
        }

        private void ApplyPaging(SimpleQuery query, List<ICommandBuilder> commandBuilders, ICommandBuilder mainCommandBuilder, SkipClause skipClause, TakeClause takeClause, bool hasWithClause, IQueryPager queryPager)
        {
            const int maxInt = 2147483646;

            IEnumerable<string> commandTexts;
            if (skipClause == null && !hasWithClause)
            {
                commandTexts = queryPager.ApplyLimit(mainCommandBuilder.Text, takeClause.Count);
            }
            else
            {
                var table = _adapter.GetSchema().FindTable(query.TableName);
                var keys = new string[0];
                if (table.PrimaryKey != null && table.PrimaryKey.Length > 0)
                {
                    keys = table.PrimaryKey.AsEnumerable()
                        .Select(k => string.Format("{0}.{1}", table.QualifiedName, _adapter.GetSchema().QuoteObjectName(k)))
                        .ToArray();
                }

                int skip = skipClause == null ? 0 : skipClause.Count;
                int take = takeClause == null ? maxInt : takeClause.Count;
                commandTexts = queryPager.ApplyPaging(mainCommandBuilder.Text, keys,  skip, take);
            }

            commandBuilders.AddRange(
                commandTexts.Select(
                    commandText =>
                    new CommandBuilder(commandText, _adapter.GetSchema(), mainCommandBuilder.Parameters)));
        }

        private ICommandBuilder[] GetQueryCommandBuilders(ref SimpleQuery query, List<SimpleQueryClauseBase> unhandledClauses)
        {
            if (query.Clauses.OfType<TakeClause>().Any() || query.Clauses.OfType<SkipClause>().Any())
            {
                return GetPagedQueryCommandBuilders(ref query, unhandledClauses);
            }
            return new[] {new QueryBuilder(_adapter).Build(query, unhandledClauses)};
        }

        private IEnumerable<ICommandBuilder> GetQueryCommandBuilders(ref SimpleQuery query, Int32 bulkIndex,
                                                                     List<SimpleQueryClauseBase> unhandledClauses)
        {
            if (query.Clauses.OfType<TakeClause>().Any() || query.Clauses.OfType<SkipClause>().Any())
            {
                return GetPagedQueryCommandBuilders(ref query, bulkIndex, unhandledClauses);
            }
            return new[] {new QueryBuilder(_adapter, bulkIndex).Build(query, unhandledClauses)};
        }

        public IEnumerable<Task<IEnumerable<IDictionary<string, object>>>> RunQueries(SimpleQuery[] queries, List <IEnumerable<SimpleQueryClauseBase>> unhandledClauses)
        {
            if (_adapter.ProviderSupportsCompoundStatements && queries.Length > 1)
            {
                var commandBuilders = new List<ICommandBuilder>();
                for (int i = 0; i < queries.Length; i++)
                {
                    var unhandledClausesForThisQuery = new List<SimpleQueryClauseBase>();
                    commandBuilders.AddRange(GetQueryCommandBuilders(ref queries[i], i, unhandledClausesForThisQuery));
                    unhandledClauses.Add(unhandledClausesForThisQuery);
                }
                IDbConnection connection;
                if (_transaction != null)
                {
                    connection = _transaction.DbTransaction.Connection;
                }
                else
                {
                    connection = _adapter.CreateConnection();
                }
                IDbCommand command =
                    new CommandBuilder(_adapter.GetSchema()).CreateCommand(
                        _adapter.ProviderHelper.GetCustomProvider<IDbParameterFactory>(_adapter.SchemaProvider),
                        commandBuilders.ToArray(), connection, _adapter.AdoOptions);
                if (_transaction != null)
                {
                    command.Transaction = _transaction.DbTransaction;
                }
                foreach (var item in command.ToEnumerables(connection))
                {
                    yield return item;
                }
            }
            else
            {
                foreach (SimpleQuery t in queries)
                {
                    var unhandledClausesForThisQuery = new List<SimpleQueryClauseBase>();
                    yield return RunQuery(t, unhandledClausesForThisQuery);
                    unhandledClauses.Add(unhandledClausesForThisQuery);
                }
            }
        }
    }
}