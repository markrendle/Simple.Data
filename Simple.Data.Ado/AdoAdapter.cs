using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    [Export("Ado", typeof (Adapter))]
    public partial class AdoAdapter : Adapter, IAdapterWithRelation, IAdapterWithTransactions, ICloneable
    {
        private readonly AdoAdapterFinder _finder;
        private readonly ProviderHelper _providerHelper = new ProviderHelper();
        private CommandOptimizer _commandOptimizer = new CommandOptimizer();

        private IConnectionProvider _connectionProvider;

        private Lazy<AdoAdapterRelatedFinder> _relatedFinder;
        private DatabaseSchema _schema;
        private IDbConnection _sharedConnection;

        public AdoAdapter()
        {
            _finder = new AdoAdapterFinder(this);
        }

        internal AdoAdapter(IConnectionProvider connectionProvider) : this()
        {
            _connectionProvider = connectionProvider;
            _schema = DatabaseSchema.Get(_connectionProvider, _providerHelper);
            _relatedFinder = new Lazy<AdoAdapterRelatedFinder>(CreateRelatedFinder);
            _commandOptimizer = ProviderHelper.GetCustomProvider<CommandOptimizer>(_connectionProvider) ??
                                new CommandOptimizer();
        }

        private AdoAdapter(IConnectionProvider connectionProvider, AdoAdapterFinder finder, ProviderHelper providerHelper,
            Lazy<AdoAdapterRelatedFinder> relatedFinder, DatabaseSchema schema)
        {
            _connectionProvider = connectionProvider;
            _finder = finder;
            _providerHelper = providerHelper;
            _relatedFinder = relatedFinder;
            _schema = schema;
        }

        public CommandOptimizer CommandOptimizer
        {
            get { return _commandOptimizer; }
        }

        public ProviderHelper ProviderHelper
        {
            get { return _providerHelper; }
        }

        public IConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        internal AdoAdapterFinder Finder
        {
            get { return _finder; }
        }

        public bool ProviderSupportsCompoundStatements
        {
            get { return _connectionProvider.SupportsCompoundStatements; }
        }

        public ISchemaProvider SchemaProvider
        {
            get { return _connectionProvider.GetSchemaProvider(); }
        }

        #region IAdapterWithRelation Members

        /// <summary>
        /// Determines whether a relation is valid.
        /// </summary>
        /// <param name="tableName">Name of the known table.</param>
        /// <param name="relatedTableName">Name of the table to test.</param>
        /// <returns>
        /// 	<c>true</c> if there is a valid relation; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidRelation(string tableName, string relatedTableName)
        {
            return _relatedFinder.Value.IsValidRelation(tableName, relatedTableName);
        }

        /// <summary>
        /// Finds data from a "table" related to the specified "table".
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="row"></param>
        /// <param name="relatedTableName"></param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        /// <remarks>When implementing the <see cref="Adapter"/> interface, if relationships are not possible, throw a <see cref="NotSupportedException"/>.</remarks>
        public object FindRelated(string tableName, IDictionary<string, object> row, string relatedTableName)
        {
            return _relatedFinder.Value.FindRelated(tableName, row, relatedTableName);
        }

        #endregion

        #region IAdapterWithTransactions Members

        public IEnumerable<IDictionary<string, object>> InsertMany(string tableName,
                                                                   IEnumerable<IDictionary<string, object>> data,
                                                                   IAdapterTransaction transaction,
                                                                   Func<IDictionary<string,object>,Exception,bool> onError, bool resultRequired)
        {
            return new AdoAdapterInserter(this, ((AdoAdapterTransaction) transaction).Transaction).InsertMany(
                tableName, data, onError, resultRequired);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> data,
                              IAdapterTransaction transaction)
        {
            IBulkUpdater bulkUpdater = ProviderHelper.GetCustomProvider<IBulkUpdater>(ConnectionProvider) ??
                                       new BulkUpdater();
            return bulkUpdater.Update(this, tableName, data.ToList(), ((AdoAdapterTransaction) transaction).Transaction);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> data,
                              IAdapterTransaction transaction, IList<string> keyFields)
        {
            IBulkUpdater bulkUpdater = ProviderHelper.GetCustomProvider<IBulkUpdater>(ConnectionProvider) ??
                                       new BulkUpdater();
            return bulkUpdater.Update(this, tableName, data.ToList(), ((AdoAdapterTransaction) transaction).Transaction);
        }

        public int Update(string tableName, IDictionary<string, object> data, IAdapterTransaction adapterTransaction)
        {
            string[] keyFieldNames = GetKeyFieldNames(tableName).ToArray();
            if (keyFieldNames.Length == 0) throw new AdoAdapterException("No Primary Key found for implicit update");
            return Update(tableName, data, GetCriteria(tableName, keyFieldNames, data), adapterTransaction);
        }

        public int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList,
                              IEnumerable<string> criteriaFieldNames, IAdapterTransaction adapterTransaction)
        {
            IBulkUpdater bulkUpdater = ProviderHelper.GetCustomProvider<IBulkUpdater>(ConnectionProvider) ??
                                       new BulkUpdater();
            return bulkUpdater.Update(this, tableName, dataList, criteriaFieldNames,
                                      ((AdoAdapterTransaction) adapterTransaction).Transaction);
        }

        public IAdapterTransaction BeginTransaction()
        {
            IDbConnection connection = CreateConnection();
            connection.OpenIfClosed();
            IDbTransaction transaction = connection.BeginTransaction();
            return new AdoAdapterTransaction(transaction);
        }

        public IAdapterTransaction BeginTransaction(string name)
        {
            IDbConnection connection = CreateConnection();
            connection.OpenIfClosed();
            var sqlConnection = connection as SqlConnection;
            IDbTransaction transaction = sqlConnection != null
                                             ? sqlConnection.BeginTransaction(name)
                                             : connection.BeginTransaction();

            return new AdoAdapterTransaction(transaction, name);
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria,
                                                             IAdapterTransaction transaction)
        {
            return new AdoAdapterFinder(this, ((AdoAdapterTransaction) transaction).Transaction).Find(tableName,
                                                                                                      criteria);
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data,
                                                  IAdapterTransaction transaction, bool resultRequired)
        {
            return new AdoAdapterInserter(this, ((AdoAdapterTransaction) transaction).Transaction).Insert(tableName,
                                                                                                          data, resultRequired);
        }

        public int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria,
                          IAdapterTransaction transaction)
        {
            ICommandBuilder commandBuilder = new UpdateHelper(_schema).GetUpdateCommand(tableName, data, criteria);
            return Execute(commandBuilder, transaction);
        }

        public int Delete(string tableName, SimpleExpression criteria, IAdapterTransaction transaction)
        {
            ICommandBuilder commandBuilder = new DeleteHelper(_schema).GetDeleteCommand(tableName, criteria);
            return Execute(commandBuilder, transaction);
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return new AdoAdapter(_connectionProvider);
        }

        #endregion

        protected override void OnSetup()
        {
            ICollection<string> settingsKeys = ((IDictionary<string, object>) Settings).Keys;
            if (settingsKeys.Contains("ConnectionString"))
            {
                if (settingsKeys.Contains("ProviderName"))
                {
                    _connectionProvider = ProviderHelper.GetProviderByConnectionString(Settings.ConnectionString,
                                                                                       Settings.ProviderName);
                }
                else
                {
                    _connectionProvider = ProviderHelper.GetProviderByConnectionString(Settings.ConnectionString);
                }
            }
            else if (settingsKeys.Contains("Filename"))
            {
                _connectionProvider = ProviderHelper.GetProviderByFilename(Settings.Filename);
            }
            else if (settingsKeys.Contains("ConnectionName"))
            {
                _connectionProvider = ProviderHelper.GetProviderByConnectionName(Settings.ConnectionName);
            }
            _schema = DatabaseSchema.Get(_connectionProvider, _providerHelper);
            _relatedFinder = new Lazy<AdoAdapterRelatedFinder>(CreateRelatedFinder);
            _commandOptimizer = ProviderHelper.GetCustomProvider<CommandOptimizer>(_connectionProvider) ??
                                new CommandOptimizer();
        }

        private AdoAdapterRelatedFinder CreateRelatedFinder()
        {
            return new AdoAdapterRelatedFinder(this);
        }

        public override IDictionary<string, object> Get(string tableName, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> FindOne(string tableName, SimpleExpression criteria)
        {
            return _finder.FindOne(tableName, criteria);
        }

        public override Func<object[], IDictionary<string, object>> CreateFindOneDelegate(string tableName,
                                                                                          SimpleExpression criteria)
        {
            return _finder.CreateFindOneDelegate(tableName, criteria);
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return _finder.Find(tableName, criteria);
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query,
                                                                          out IEnumerable<SimpleQueryClauseBase>
                                                                              unhandledClauses)
        {
            if (query.Clauses.OfType<WithCountClause>().Any()) return RunQueryWithCount(query, out unhandledClauses);

            ICommandBuilder[] commandBuilders = GetQueryCommandBuilders(query, out unhandledClauses);
            IDbConnection connection = CreateConnection();
            if (ProviderSupportsCompoundStatements || commandBuilders.Length == 1)
            {
                return
                    CommandBuilder.CreateCommand(
                        _providerHelper.GetCustomProvider<IDbParameterFactory>(_schema.SchemaProvider), commandBuilders,
                        connection).ToEnumerable(this.CreateConnection);
            }
            else
            {
                return commandBuilders.SelectMany(cb => cb.GetCommand(connection).ToEnumerable(this.CreateConnection));
            }
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
            SimpleQuery countQuery =
                query.ClearSkip().ClearTake().ClearOrderBy().ReplaceSelect(new CountSpecialReference());
            var unhandledClausesList = new List<IEnumerable<SimpleQueryClauseBase>>
                                           {
                                               Enumerable.Empty<SimpleQueryClauseBase>(),
                                               Enumerable.Empty<SimpleQueryClauseBase>()
                                           };

            using (
                IEnumerator<IEnumerable<IDictionary<string, object>>> enumerator =
                    RunQueries(new[] {countQuery, query}, unhandledClausesList).GetEnumerator())
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

        private ICommandBuilder[] GetPagedQueryCommandBuilders(SimpleQuery query,
                                                               out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return GetPagedQueryCommandBuilders(query, -1, out unhandledClauses);
        }

        private ICommandBuilder[] GetPagedQueryCommandBuilders(SimpleQuery query, Int32 bulkIndex,
                                                               out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            var commandBuilders = new List<ICommandBuilder>();
            var unhandledClausesList = new List<SimpleQueryClauseBase>();
            unhandledClauses = unhandledClausesList;

            IEnumerable<SimpleQueryClauseBase> unhandledClausesForPagedQuery;
            ICommandBuilder mainCommandBuilder = new QueryBuilder(this, bulkIndex).Build(query,
                                                                                         out
                                                                                             unhandledClausesForPagedQuery);
            unhandledClausesList.AddRange(unhandledClausesForPagedQuery);

            const int maxInt = 2147483646;

            SkipClause skipClause = query.Clauses.OfType<SkipClause>().FirstOrDefault() ?? new SkipClause(0);
            TakeClause takeClause = query.Clauses.OfType<TakeClause>().FirstOrDefault() ?? new TakeClause(maxInt);

            if (skipClause.Count != 0 || takeClause.Count != maxInt)
            {
                var queryPager = ProviderHelper.GetCustomProvider<IQueryPager>(ConnectionProvider);
                if (queryPager == null)
                {
                    unhandledClausesList.AddRange(query.OfType<SkipClause>());
                    unhandledClausesList.AddRange(query.OfType<TakeClause>());
                }

                IEnumerable<string> commandTexts = queryPager.ApplyPaging(mainCommandBuilder.Text, skipClause.Count,
                                                                          takeClause.Count);

                foreach (string commandText in commandTexts)
                {
                    var commandBuilder = new CommandBuilder(commandText, _schema, mainCommandBuilder.Parameters);
                    commandBuilders.Add(commandBuilder);
                }
            }
            return commandBuilders.ToArray();
        }

        private ICommandBuilder[] GetQueryCommandBuilders(SimpleQuery query,
                                                          out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            if (query.Clauses.OfType<TakeClause>().Any() || query.Clauses.OfType<SkipClause>().Any())
            {
                return GetPagedQueryCommandBuilders(query, out unhandledClauses);
            }
            else
            {
                return new[] {new QueryBuilder(this).Build(query, out unhandledClauses)};
            }
        }

        private ICommandBuilder[] GetQueryCommandBuilders(SimpleQuery query, Int32 bulkIndex,
                                                          out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            if (query.Clauses.OfType<TakeClause>().Any() || query.Clauses.OfType<SkipClause>().Any())
            {
                return GetPagedQueryCommandBuilders(query, bulkIndex, out unhandledClauses);
            }
            else
            {
                return new[] {new QueryBuilder(this, bulkIndex).Build(query, out unhandledClauses)};
            }
        }

        public override IEnumerable<IEnumerable<IDictionary<string, object>>> RunQueries(SimpleQuery[] queries,
                                                                                         List
                                                                                             <
                                                                                             IEnumerable
                                                                                             <SimpleQueryClauseBase>>
                                                                                             unhandledClauses)
        {
            if (ProviderSupportsCompoundStatements && queries.Length > 1)
            {
                var commandBuilders = new List<ICommandBuilder>();
                for (int i = 0; i < queries.Length; i++)
                {
                    IEnumerable<SimpleQueryClauseBase> unhandledClausesForThisQuery;
                    commandBuilders.AddRange(GetQueryCommandBuilders(queries[i], i, out unhandledClausesForThisQuery));
                    unhandledClauses.Add(unhandledClausesForThisQuery);
                }
                IDbConnection connection = CreateConnection();
                IDbCommand command =
                    CommandBuilder.CreateCommand(
                        _providerHelper.GetCustomProvider<IDbParameterFactory>(_schema.SchemaProvider),
                        commandBuilders.ToArray(), connection);
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

        public override bool IsExpressionFunction(string functionName, params object[] args)
        {
            return functionName.Equals("like", StringComparison.OrdinalIgnoreCase)
                   && args.Length == 1
                   && args[0] is string;
        }

        public override IObservable<IDictionary<string, object>> RunQueryAsObservable(SimpleQuery query,
                                                                                      out
                                                                                          IEnumerable
                                                                                          <SimpleQueryClauseBase>
                                                                                          unhandledClauses)
        {
            IDbConnection connection = CreateConnection();
            return new QueryBuilder(this).Build(query, out unhandledClauses)
                .GetCommand(connection)
                .ToObservable(connection, this);
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired)
        {
            return new AdoAdapterInserter(this).Insert(tableName, data, resultRequired);
        }

        public override IEnumerable<IDictionary<string, object>> InsertMany(string tableName,
                                                                            IEnumerable<IDictionary<string, object>>
                                                                                data, Func<IDictionary<string,object>, Exception, bool> onError, bool resultRequired)
        {
            return new AdoAdapterInserter(this).InsertMany(tableName, data, onError, resultRequired);
        }

        public override int Update(string tableName, IDictionary<string, object> data)
        {
            string[] keyFieldNames = GetKeyFieldNames(tableName).ToArray();
            if (keyFieldNames.Length == 0) throw new AdoAdapterException("No Primary Key found for implicit update");
            return Update(tableName, data, GetCriteria(tableName, keyFieldNames, data));
        }


        public override int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> data,
                                       IEnumerable<string> criteriaFieldNames)
        {
            IBulkUpdater bulkUpdater = ProviderHelper.GetCustomProvider<IBulkUpdater>(ConnectionProvider) ??
                                       new BulkUpdater();
            return bulkUpdater.Update(this, tableName, data.ToList(), criteriaFieldNames, null);
        }

        public override int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> data)
        {
            IBulkUpdater bulkUpdater = ProviderHelper.GetCustomProvider<IBulkUpdater>(ConnectionProvider) ??
                                       new BulkUpdater();
            return bulkUpdater.Update(this, tableName, data.ToList(), null);
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            ICommandBuilder commandBuilder = new UpdateHelper(_schema).GetUpdateCommand(tableName, data, criteria);
            return Execute(commandBuilder);
        }

        /// <summary>
        /// Deletes from the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The expression to use as criteria for the delete operation.</param>
        /// <returns>The number of records which were deleted.</returns>
        public override int Delete(string tableName, SimpleExpression criteria)
        {
            ICommandBuilder commandBuilder = new DeleteHelper(_schema).GetDeleteCommand(tableName, criteria);
            return Execute(commandBuilder);
        }

        /// <summary>
        /// Gets the names of the fields which comprise the unique identifier for the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A list of field names; an empty list if no key is defined.</returns>
        public IEnumerable<string> GetKeyFieldNames(string tableName)
        {
            return _schema.FindTable(tableName).PrimaryKey.AsEnumerable();
        }

        private int Execute(ICommandBuilder commandBuilder)
        {
            IDbConnection connection = CreateConnection();
            using (connection.MaybeDisposable())
            {
                using (IDbCommand command = commandBuilder.GetCommand(connection))
                {
                    connection.OpenIfClosed();
                    return TryExecute(command);
                }
            }
        }

        private static int Execute(ICommandBuilder commandBuilder, IAdapterTransaction transaction)
        {
            IDbTransaction dbTransaction = ((AdoAdapterTransaction) transaction).Transaction;
            using (IDbCommand command = commandBuilder.GetCommand(dbTransaction.Connection))
            {
                command.Transaction = dbTransaction;
                return TryExecute(command);
            }
        }

        private static int TryExecute(IDbCommand command)
        {
            command.WriteTrace();
            try
            {
                return command.ExecuteNonQuery();
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }

        public void UseSharedConnection(IDbConnection connection)
        {
            _sharedConnection = connection;
        }

        public void StopUsingSharedConnection()
        {
            _sharedConnection = null;
        }

        public IDbConnection CreateConnection()
        {
            return _sharedConnection ?? _connectionProvider.CreateConnection();
        }

        public DatabaseSchema GetSchema()
        {
            return _schema ?? (_schema = DatabaseSchema.Get(_connectionProvider, _providerHelper));
        }

        public IAdapterTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            IDbConnection connection = CreateConnection();
            connection.OpenIfClosed();
            IDbTransaction transaction = connection.BeginTransaction(isolationLevel);
            return new AdoAdapterTransaction(transaction);
        }

        public IAdapterTransaction BeginTransaction(IsolationLevel isolationLevel, string name)
        {
            IDbConnection connection = CreateConnection();
            connection.OpenIfClosed();
            var sqlConnection = connection as SqlConnection;
            IDbTransaction transaction = sqlConnection != null
                                             ? sqlConnection.BeginTransaction(isolationLevel, name)
                                             : connection.BeginTransaction(isolationLevel);

            return new AdoAdapterTransaction(transaction, name);
        }

        public string GetIdentityFunction()
        {
            return _connectionProvider.GetIdentityFunction();
        }

        protected override void OnReset()
        {
            DatabaseSchema.ClearCache();
            _schema = DatabaseSchema.Get(_connectionProvider, _providerHelper);
        }
    }
}