using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    /// <summary>
    /// Provides an abstraction over the underlying data adapter, if it is transaction-capable.
    /// </summary>
    public sealed class SimpleTransaction : DataStrategy, IDisposable
    {
        private readonly Database _database;

        private readonly IAdapterWithTransactions _adapter;
        private IAdapterTransaction _adapterTransaction;

        private SimpleTransaction(IAdapterWithTransactions adapter, Database database)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (database == null) throw new ArgumentNullException("database");
            _adapter = adapter;
            _database = database;
        }

        private void Begin()
        {
            _adapterTransaction = _adapter.BeginTransaction();
        }

        private void Begin(string name)
        {
            _adapterTransaction = _adapter.BeginTransaction(name);
        }

        internal static SimpleTransaction Begin(Database database)
        {
            SimpleTransaction transaction = CreateTransaction(database);
            transaction.Begin();
            return transaction;
        }

        internal static SimpleTransaction Begin(Database database, string name)
        {
            SimpleTransaction transaction = CreateTransaction(database);
            transaction.Begin(name);
            return transaction;
        }

        private static SimpleTransaction CreateTransaction(Database database)
        {
            var adapterWithTransactions = database.GetAdapter() as IAdapterWithTransactions;
            if (adapterWithTransactions == null) throw new NotSupportedException();
            return new SimpleTransaction(adapterWithTransactions, database);
        }


        internal Database Database
        {
            get { return _database; }
        }

        /// <summary>
        /// Gets the name assigned to the transaction.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return _adapterTransaction.Name; }
        }

        public IAdapterTransaction AdapterTransaction
        {
            get { return _adapterTransaction; }
        }

        /// <summary>
        /// Commits all changes to the database and cleans up resources associated with the transaction.
        /// </summary>
        public void Commit()
        {
            _adapterTransaction.Commit();
        }

        /// <summary>
        /// Rolls back all changes to the database and cleans up resources associated with the transaction.
        /// </summary>
        public void Rollback()
        {
            _adapterTransaction.Rollback();
        }

        internal override IDictionary<string, object> FindOne(string tableName, SimpleExpression criteria)
        {
            return Find(tableName, criteria).FirstOrDefault();
        }

        internal override int Update(string tableName, IList<IDictionary<string, object>> dataList)
        {
            return _adapter.UpdateMany(tableName, dataList, AdapterTransaction);
        }

        internal override int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IList<string> keyFields)
        {
            return _adapter.UpdateMany(tableName, dataList, _adapterTransaction, keyFields);
        }


        internal override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return _adapter.Find(tableName, criteria, AdapterTransaction);
        }

        internal override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            return _adapter.Insert(tableName, data, AdapterTransaction);
        }

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        internal override IEnumerable<IDictionary<string, object>> Insert(string tableName, IEnumerable<IDictionary<string, object>> data)
        {
            return _adapter.InsertMany(tableName, data, AdapterTransaction);
        }

        internal override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            return _adapter.Update(tableName, data, criteria, AdapterTransaction);
        }

        internal override int Delete(string tableName, SimpleExpression criteria)
        {
            return _adapter.Delete(tableName, criteria, AdapterTransaction);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _adapterTransaction.Dispose();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("IAdapterTransaction Dispose threw exception: " + ex.Message);
            }
        }

        public override Adapter GetAdapter()
        {
            return _adapter as Adapter;
        }

        protected internal override Database GetDatabase()
        {
            return _database;
        }
    }
}
