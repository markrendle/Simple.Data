namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;

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

        public IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query,
                                                                 out IEnumerable<SimpleQueryClauseBase>
                                                                     unhandledClauses)
        {
            IEnumerable<IDictionary<string, object>> result;

            if (query.Clauses.OfType<WithCountClause>().Any()) return RunQueryWithCount(query, out unhandledClauses);

            ICommandBuilder[] commandBuilders = GetQueryCommandBuilders(ref query, out unhandledClauses);
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

        public IObservable<IDictionary<string, object>> RunQueryAsObservable(SimpleQuery query,
                                                                             out
                                                                                 IEnumerable
                                                                                 <SimpleQueryClauseBase>
                                                                                 unhandledClauses)
        {
            IDbConnection connection = _adapter.CreateConnection();
            return new QueryBuilder(_adapter).Build(query, out unhandledClauses)
                .GetCommand(connection, _adapter.AdoOptions)
                .ToObservable(connection, _adapter);
        }

        private IEnumerable<IDictionary<string, object>> RunQueryWithCount(SimpleQuery query,
                                                                           out IEnumerable<SimpleQueryClauseBase>
                                                                               unhandledClauses)
        {
            WithCountClause withCountClause;
            try
            {
                withCountClause = query.Clauses.OfType<WithCountClause>().First();
            }
            catch (InvalidOperationException)
            {
                // Rethrow with meaning.
                throw new InvalidOperationException("No WithCountClause specified.");
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
                unhandledClauses = unhandledClausesList[1];
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException();
                }
                IDictionary<string, object> countRow = enumerator.Current.Single();
                withCountClause.SetCount((int) countRow.First().Value);
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException();
                }
                return enumerator.Current;
            }
        }

        private ICommandBuilder[] GetPagedQueryCommandBuilders(ref SimpleQuery query,
                                                               out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return GetPagedQueryCommandBuilders(ref query, -1, out unhandledClauses);
        }

        private ICommandBuilder[] GetPagedQueryCommandBuilders(ref SimpleQuery query, Int32 bulkIndex,
                                                               out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var commandBuilders = new List<ICommandBuilder>();
            var unhandledClausesList = new List<SimpleQueryClauseBase>();
            unhandledClauses = unhandledClausesList;

            IEnumerable<SimpleQueryClauseBase> unhandledClausesForPagedQuery;
            ICommandBuilder mainCommandBuilder = new QueryBuilder(_adapter, bulkIndex).Build(query,
                                                                                             out
                                                                                                 unhandledClausesForPagedQuery);
            unhandledClausesList.AddRange(unhandledClausesForPagedQuery);

            SkipClause skipClause = query.Clauses.OfType<SkipClause>().FirstOrDefault();
            TakeClause takeClause = query.Clauses.OfType<TakeClause>().FirstOrDefault();

            if (skipClause != null || takeClause != null)
            {
                var queryPager = _adapter.ProviderHelper.GetCustomProvider<IQueryPager>(_adapter.ConnectionProvider);
                if (queryPager == null)
                {
                    Trace.TraceWarning("There is no database-specific query paging in your current Simple.Data Provider. Paging will be done in memory.");
                    DeferPaging(ref query, mainCommandBuilder, commandBuilders, unhandledClausesList);
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
                if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
                {
                    throw new AdoAdapterException("Cannot apply paging to a table with no primary key.");
                }
                var keys = table.PrimaryKey.AsEnumerable()
                     .Select(k => string.Format("{0}.{1}", table.QualifiedName, _adapter.GetSchema().QuoteObjectName(k)))
                     .ToArray();
                int skip = skipClause == null ? 0 : skipClause.Count;
                int take = takeClause == null ? maxInt : takeClause.Count;
                commandTexts = queryPager.ApplyPaging(mainCommandBuilder.Text, keys,  skip, take);
            }

            commandBuilders.AddRange(
                commandTexts.Select(
                    commandText =>
                    new CommandBuilder(commandText, _adapter.GetSchema(), mainCommandBuilder.Parameters)));
        }

        private ICommandBuilder[] GetQueryCommandBuilders(ref SimpleQuery query,
                                                          out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            if (query.Clauses.OfType<TakeClause>().Any() || query.Clauses.OfType<SkipClause>().Any())
            {
                return GetPagedQueryCommandBuilders(ref query, out unhandledClauses);
            }
            return new[] {new QueryBuilder(_adapter).Build(query, out unhandledClauses)};
        }

        private IEnumerable<ICommandBuilder> GetQueryCommandBuilders(ref SimpleQuery query, Int32 bulkIndex,
                                                                     out IEnumerable<SimpleQueryClauseBase>
                                                                         unhandledClauses)
        {
            if (query.Clauses.OfType<TakeClause>().Any() || query.Clauses.OfType<SkipClause>().Any())
            {
                return GetPagedQueryCommandBuilders(ref query, bulkIndex, out unhandledClauses);
            }
            return new[] {new QueryBuilder(_adapter, bulkIndex).Build(query, out unhandledClauses)};
        }

        public IEnumerable<IEnumerable<IDictionary<string, object>>> RunQueries(SimpleQuery[] queries,
                                                                                List
                                                                                    <
                                                                                    IEnumerable
                                                                                    <SimpleQueryClauseBase>>
                                                                                    unhandledClauses)
        {
            if (_adapter.ProviderSupportsCompoundStatements && queries.Length > 1)
            {
                var commandBuilders = new List<ICommandBuilder>();
                for (int i = 0; i < queries.Length; i++)
                {
                    IEnumerable<SimpleQueryClauseBase> unhandledClausesForThisQuery;
                    commandBuilders.AddRange(GetQueryCommandBuilders(ref queries[i], i, out unhandledClausesForThisQuery));
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
                    yield return item.ToList();
                }
            }
            else
            {
                foreach (SimpleQuery t in queries)
                {
                    IEnumerable<SimpleQueryClauseBase> unhandledClausesForThisQuery;
                    yield return RunQuery(t, out unhandledClausesForThisQuery);
                    unhandledClauses.Add(unhandledClausesForThisQuery);
                }
            }
        }
    }
}