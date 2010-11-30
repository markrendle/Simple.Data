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
        private readonly IAdapterTransaction _adapterTransaction;

        internal SimpleTransaction(IAdapterWithTransactions adapter, IAdapterTransaction adapterTransaction, Database database)
        {
            if (adapterTransaction == null) throw new ArgumentNullException("adapterTransaction");
            _adapter = adapter;
            _adapterTransaction = adapterTransaction;
            _database = database;
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

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return _adapter.Find(tableName, criteria, AdapterTransaction);
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            return _adapter.Insert(tableName, data, AdapterTransaction);
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            return _adapter.Update(tableName, data, criteria, AdapterTransaction);
        }

        public override int Delete(string tableName, SimpleExpression criteria)
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

        protected override Adapter GetAdapter()
        {
            return _adapter as Adapter;
        }
    }
}
