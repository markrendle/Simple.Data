using System;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Text;

namespace Shitty.Data
{
    using Commands;

    /// <summary>
    /// Provides an abstraction over the underlying data adapter, if it is transaction-capable.
    /// </summary>
    public sealed class SimpleTransaction : DataStrategy, IDisposable
    {
        private readonly DataStrategy _database;
        private readonly IsolationLevel _isolationLevel;

        private readonly IAdapterWithTransactions _adapter;
        private TransactionRunner _transactionRunner;
        private IAdapterTransaction _adapterTransaction;

        private SimpleTransaction(IAdapterWithTransactions adapter, DataStrategy database, IsolationLevel isolationLevel)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            if (database == null) throw new ArgumentNullException("database");
            _adapter = adapter;
            _database = database;
            _isolationLevel = isolationLevel;
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
            _adapterTransaction = _adapter.BeginTransaction(_isolationLevel);
            _transactionRunner = new TransactionRunner(_adapter, _adapterTransaction);
        }

        private void Begin(string name)
        {
            _adapterTransaction = _adapter.BeginTransaction(name, _isolationLevel);
            _transactionRunner = new TransactionRunner(_adapter, _adapterTransaction);
        }

        internal static SimpleTransaction Begin(DataStrategy database)
        {
            SimpleTransaction transaction = CreateTransaction(database);
            transaction.Begin();
            return transaction;
        }

        internal static SimpleTransaction Begin(DataStrategy database, string name)
        {
            SimpleTransaction transaction = CreateTransaction(database);
            transaction.Begin(name);
            return transaction;
        }

        public static SimpleTransaction Begin(DataStrategy database, IsolationLevel isolationLevel)
        {
            var transaction = CreateTransaction(database, isolationLevel);
            transaction.Begin();
            return transaction;
        }

        private static SimpleTransaction CreateTransaction(DataStrategy database, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            var adapterWithTransactions = database.GetAdapter() as IAdapterWithTransactions;
            if (adapterWithTransactions == null) throw new NotSupportedException();
            return new SimpleTransaction(adapterWithTransactions, database, isolationLevel);
        }

        internal DataStrategy Database
        {
            get
            {
                return _database;
            }
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
                SimpleDataTraceSources.TraceSource.TraceEvent(TraceEventType.Error, SimpleDataTraceSources.GenericErrorMessageId,
                    "IAdapterTransaction Dispose threw exception: {0}", ex.Message);
            }
        }

        public override Adapter GetAdapter()
        {
            return _adapter as Adapter;
        }

        protected internal override bool ExecuteFunction(out object result, ExecuteFunctionCommand command)
        {
            return command.Execute(out result, _adapterTransaction);
        }

        protected internal override DataStrategy GetDatabase()
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
