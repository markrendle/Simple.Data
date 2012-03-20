using System;
using System.Diagnostics;
using System.Dynamic;
using System.Text;

namespace Simple.Data
{
    using Commands;

    /// <summary>
    /// Provides an abstraction over the underlying data adapter, if it is transaction-capable.
    /// </summary>
    public sealed class SimpleTransaction : DataStrategy, IDisposable
    {
        private readonly Database _database;

        private readonly IAdapterWithTransactions _adapter;
        private TransactionRunner _transactionRunner;
        private IAdapterTransaction _adapterTransaction;

        private SimpleTransaction(IAdapterWithTransactions adapter, Database database)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (database == null) throw new ArgumentNullException("database");
            _adapter = adapter;
            _database = database;
        }

        private SimpleTransaction(SimpleTransaction copy) : base(copy)
        {
            _adapter = copy._adapter;
            _database = copy._database;
            _adapterTransaction = copy._adapterTransaction;
            _transactionRunner = copy._transactionRunner;
        }

        private void Begin()
        {
            _adapterTransaction = _adapter.BeginTransaction();
            _transactionRunner = new TransactionRunner(_adapter, _adapterTransaction);
        }

        private void Begin(string name)
        {
            _adapterTransaction = _adapter.BeginTransaction(name);
            _transactionRunner = new TransactionRunner(_adapter, _adapterTransaction);
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

        protected override bool ExecuteFunction(out object result, ExecuteFunctionCommand command)
        {
            return command.Execute(out result, _adapterTransaction);
        }

        protected internal override Database GetDatabase()
        {
            return _database;
        }

        internal override RunStrategy Run
        {
            get { return _transactionRunner; }
        }

        protected internal override DataStrategy Clone()
        {
            return new SimpleTransaction(this);
        }
    }
}
