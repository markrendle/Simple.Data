using Simple.Data.Operations;

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
        public IAdapterTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            IDbConnection connection = CreateConnection();
            connection.OpenIfClosed();
            IDbTransaction transaction = connection.BeginTransaction(isolationLevel);
            return new AdoAdapterTransaction(transaction, _sharedConnection != null);
        }

        public IAdapterTransaction BeginTransaction(string name, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            IDbConnection connection = CreateConnection();
            connection.OpenIfClosed();
            var sqlConnection = connection as SqlConnection;
            IDbTransaction transaction = sqlConnection != null
                                             ? sqlConnection.BeginTransaction(isolationLevel, name)
                                             : connection.BeginTransaction(isolationLevel);

            return new AdoAdapterTransaction(transaction, name, _sharedConnection != null);
        }

        public OperationResult Execute(IOperation operation, IAdapterTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDictionary<string, object>> InsertMany(string tableName,
                                                                   IEnumerable<IDictionary<string, object>> data,
                                                                   IAdapterTransaction transaction,
                                                                   Func<IDictionary<string, object>, Exception, bool> onError, bool resultRequired)
        {
            return new AdoAdapterInserter(this, ((AdoAdapterTransaction)transaction).DbTransaction).InsertMany(
                tableName, data, onError, resultRequired);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> data,
                              IAdapterTransaction transaction)
        {
            IBulkUpdater bulkUpdater = ProviderHelper.GetCustomProvider<IBulkUpdater>(ConnectionProvider) ??
                                       new BulkUpdater();
            return bulkUpdater.Update(this, tableName, data.ToList(), ((AdoAdapterTransaction)transaction).DbTransaction);
        }

        public int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> data,
                              IAdapterTransaction transaction, IList<string> keyFields)
        {
            IBulkUpdater bulkUpdater = ProviderHelper.GetCustomProvider<IBulkUpdater>(ConnectionProvider) ??
                                       new BulkUpdater();
            return bulkUpdater.Update(this, tableName, data.ToList(), ((AdoAdapterTransaction)transaction).DbTransaction);
        }

        private int Update(string tableName, IDictionary<string, object> data, IAdapterTransaction adapterTransaction)
        {
            string[] keyFieldNames = GetKeyNames(tableName).ToArray();
            if (keyFieldNames.Length == 0) throw new AdoAdapterException(string.Format("No primary key found for implicit update of table '{0}'.", tableName));
            var readOnlyDictionary = data.ToReadOnly();
            return Update(tableName, data, GetCriteria(tableName, keyFieldNames, ref readOnlyDictionary), adapterTransaction);
        }

        public int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList,
                              IEnumerable<string> criteriaFieldNames, IAdapterTransaction adapterTransaction)
        {
            IBulkUpdater bulkUpdater = ProviderHelper.GetCustomProvider<IBulkUpdater>(ConnectionProvider) ??
                                       new BulkUpdater();
            return bulkUpdater.Update(this, tableName, dataList, criteriaFieldNames,
                                      ((AdoAdapterTransaction)adapterTransaction).DbTransaction);
        }

        //public IAdapterTransaction BeginTransaction()
        //{
        //    IDbConnection connection = CreateConnection();
        //    connection.OpenIfClosed();
        //    IDbTransaction transaction = connection.BeginTransaction();
        //    return new AdoAdapterTransaction(transaction, _sharedConnection != null);
        //}

        //public IAdapterTransaction BeginTransaction(string name)
        //{
        //    IDbConnection connection = CreateConnection();
        //    connection.OpenIfClosed();
        //    var sqlConnection = connection as SqlConnection;
        //    IDbTransaction transaction = sqlConnection != null
        //                                     ? sqlConnection.BeginTransaction(name)
        //                                     : connection.BeginTransaction();

        //    return new AdoAdapterTransaction(transaction, name, _sharedConnection != null);
        //}

        public IDictionary<string,object> Get(GetOperation operation, IAdapterTransaction transaction)
        {
            return new AdoAdapterGetter(this, ((AdoAdapterTransaction) transaction).DbTransaction).Get(operation.TableName,
                                                                                                     operation.KeyValues);
        }

        public IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, IAdapterTransaction transaction, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            return new AdoAdapterQueryRunner(this, (AdoAdapterTransaction)transaction).RunQuery(query, out unhandledClauses);
        }

        public IEnumerable<IDictionary<string, object>> Find(QueryOperation operation,
                                                             IAdapterTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data,
                                                  IAdapterTransaction transaction, bool resultRequired)
        {
            return new AdoAdapterInserter(this, ((AdoAdapterTransaction)transaction).DbTransaction).Insert(tableName,
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

        private IDictionary<string, object> Upsert(string tableName, IDictionary<string, object> data, SimpleExpression criteria, bool resultRequired, IAdapterTransaction adapterTransaction)
        {
            var transaction = ((AdoAdapterTransaction) adapterTransaction).DbTransaction;
            return new AdoAdapterUpserter(this, transaction).Upsert(tableName, data, criteria, resultRequired);
        }

        private IEnumerable<IDictionary<string, object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list, IAdapterTransaction adapterTransaction, bool isResultRequired, Func<IDictionary<string, object>, Exception, bool> errorCallback)
        {
            var transaction = ((AdoAdapterTransaction) adapterTransaction).DbTransaction;
            return new AdoAdapterUpserter(this, transaction).UpsertMany(tableName, list, isResultRequired, errorCallback);
        }

        private IEnumerable<IDictionary<string, object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list, IEnumerable<string> keyFieldNames, IAdapterTransaction adapterTransaction, bool isResultRequired, Func<IDictionary<string, object>, Exception, bool> errorCallback)
        {
            var transaction = ((AdoAdapterTransaction) adapterTransaction).DbTransaction;
            return new AdoAdapterUpserter(this, transaction).UpsertMany(tableName, list, keyFieldNames.ToArray(), isResultRequired, errorCallback);
        }
    }
}