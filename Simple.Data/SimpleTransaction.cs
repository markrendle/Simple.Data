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

        private readonly IAdapterTransaction _adapterTransaction;

        internal SimpleTransaction(IAdapterTransaction adapterTransaction, Database database)
        {
            if (adapterTransaction == null) throw new ArgumentNullException("adapterTransaction");
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

        internal override Adapter Adapter
        {
            get { return _database.Adapter; }
        }
    }
}
