using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    public abstract class DelegatingConnectionBase : IDbConnection
    {
        private readonly IDbConnection _target;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            _target.Dispose();
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <returns>
        /// An object representing the new transaction.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual IDbTransaction BeginTransaction()
        {
            return _target.BeginTransaction();
        }

        /// <summary>
        /// Begins a database transaction with the specified <see cref="T:System.Data.IsolationLevel"/> value.
        /// </summary>
        /// <returns>
        /// An object representing the new transaction.
        /// </returns>
        /// <param name="il">One of the <see cref="T:System.Data.IsolationLevel"/> values. </param><filterpriority>2</filterpriority>
        public virtual IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return _target.BeginTransaction(il);
        }

        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Close()
        {
            _target.Close();
        }

        /// <summary>
        /// Changes the current database for an open Connection object.
        /// </summary>
        /// <param name="databaseName">The name of the database to use in place of the current database. </param><filterpriority>2</filterpriority>
        public virtual void ChangeDatabase(string databaseName)
        {
            _target.ChangeDatabase(databaseName);
        }

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <returns>
        /// A Command object associated with the connection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual IDbCommand CreateCommand()
        {
            return _target.CreateCommand();
        }

        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString property of the provider-specific Connection object.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Open()
        {
            _target.Open();
        }

        /// <summary>
        /// Gets or sets the string used to open a database.
        /// </summary>
        /// <returns>
        /// A string containing connection settings.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual string ConnectionString
        {
            get { return _target.ConnectionString; }
            set { _target.ConnectionString = value; }
        }

        /// <summary>
        /// Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.
        /// </summary>
        /// <returns>
        /// The time (in seconds) to wait for a connection to open. The default value is 15 seconds.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual int ConnectionTimeout
        {
            get { return _target.ConnectionTimeout; }
        }

        /// <summary>
        /// Gets the name of the current database or the database to be used after a connection is opened.
        /// </summary>
        /// <returns>
        /// The name of the current database or the name of the database to be used once a connection is open. The default value is an empty string.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual string Database
        {
            get { return _target.Database; }
        }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.ConnectionState"/> values.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual ConnectionState State
        {
            get { return _target.State; }
        }

        protected DelegatingConnectionBase(IDbConnection target)
        {
            _target = target;
        }
    }
}
