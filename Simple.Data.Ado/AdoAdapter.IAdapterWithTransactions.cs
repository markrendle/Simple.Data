namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using Extensions;

    public partial class AdoAdapter : IAdapterWithTransactions
    {
        public IAdapterTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            IDbConnection connection = CreateConnection();
            connection.OpenIfClosed();
            IDbTransaction transaction = connection.BeginTransaction(isolationLevel);
            return new AdoAdapterTransaction(transaction, _sharedConnection != null);
        }

        public IAdapterTransaction BeginTransaction(IsolationLevel isolationLevel, string name)
        {
            IDbConnection connection = CreateConnection();
            connection.OpenIfClosed();
            var sqlConnection = connection as SqlConnection;
            IDbTransaction transaction = sqlConnection != null
                                             ? sqlConnection.BeginTransaction(isolationLevel, name)
                                             : connection.BeginTransaction(isolationLevel);

            return new AdoAdapterTransaction(transaction, name, _sharedConnection != null);
        }

        public IEnumerable<IDictionary<string, object>> InsertMany(string tableName,
                                                                   IEnumerable<IDictionary<string, object>> data,
                                                                   IAdapterTransaction transaction,
                                                                   Func<IDictionary<string, object>, Exception, bool> onError, bool resultRequired)
        {
            return new AdoAdapterInserter(this, ((AdoAdapterTransaction)transaction).Transaction).InsertMany(
                tableName, data, onError, resultRequired);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> data,
                              IAdapterTransaction transaction)
        {
            IBulkUpdater bulkUpdater = ProviderHelper.GetCustomProvider<IBulkUpdater>(ConnectionProvider) ??
                                       new BulkUpdater();
            return bulkUpdater.Update(this, tableName, data.ToList(), ((AdoAdapterTransaction)transaction).Transaction);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> data,
                              IAdapterTransaction transaction, IList<string> keyFields)
        {
            IBulkUpdater bulkUpdater = ProviderHelper.GetCustomProvider<IBulkUpdater>(ConnectionProvider) ??
                                       new BulkUpdater();
            return bulkUpdater.Update(this, tableName, data.ToList(), ((AdoAdapterTransaction)transaction).Transaction);
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
                                      ((AdoAdapterTransaction)adapterTransaction).Transaction);
        }

        public IAdapterTransaction BeginTransaction()
        {
            IDbConnection connection = CreateConnection();
            connection.OpenIfClosed();
            IDbTransaction transaction = connection.BeginTransaction();
            return new AdoAdapterTransaction(transaction, _sharedConnection != null);
        }

        public IAdapterTransaction BeginTransaction(string name)
        {
            IDbConnection connection = CreateConnection();
            connection.OpenIfClosed();
            var sqlConnection = connection as SqlConnection;
            IDbTransaction transaction = sqlConnection != null
                                             ? sqlConnection.BeginTransaction(name)
                                             : connection.BeginTransaction();

            return new AdoAdapterTransaction(transaction, name, _sharedConnection != null);
        }

        public IDictionary<string,object> Get(string tableName, IAdapterTransaction transaction, params object[] parameterValues)
        {
            return new AdoAdapterGetter(this, ((AdoAdapterTransaction) transaction).Transaction).Get(tableName,
                                                                                                     parameterValues);
        }
        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria,
                                                             IAdapterTransaction transaction)
        {
            return new AdoAdapterFinder(this, ((AdoAdapterTransaction)transaction).Transaction).Find(tableName,
                                                                                                      criteria);
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data,
                                                  IAdapterTransaction transaction, bool resultRequired)
        {
            return new AdoAdapterInserter(this, ((AdoAdapterTransaction)transaction).Transaction).Insert(tableName,
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

        public override IDictionary<string, object> Upsert(string tableName, IDictionary<string, object> data, SimpleExpression criteria, bool resultRequired, IAdapterTransaction adapterTransaction)
        {
            var transaction = ((AdoAdapterTransaction) adapterTransaction).Transaction;
            var finder = new AdoAdapterFinder(this, transaction);
            if (finder.FindOne(tableName, criteria) != null)
            {
                // Don't update columns used as criteria
                var keys = criteria.GetOperandsOfType<ObjectReference>().Select(o => o.GetName().Homogenize());
                data = data.Where(kvp => keys.All(k => k != kvp.Key.Homogenize())).ToDictionary();

                var commandBuilder = new UpdateHelper(_schema).GetUpdateCommand(tableName, data, criteria);
                Execute(commandBuilder, adapterTransaction);
                return resultRequired ? finder.FindOne(tableName, criteria) : null;
            }
            return new AdoAdapterInserter(this, transaction).Insert(tableName, data, resultRequired);
        }
    }
}