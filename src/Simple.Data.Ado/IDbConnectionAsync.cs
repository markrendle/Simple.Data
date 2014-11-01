namespace Simple.Data.Ado
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.Remoting;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDbConnectionAsync : IDbConnection
    {
        Task OpenAsync();
        Task OpenAsync(CancellationToken cancellationToken);
    }

    public class SqlConnectionAsync : IDbConnectionAsync
    {
        private readonly SqlConnection _sqlConnection;
        public object GetLifetimeService()
        {
            return _sqlConnection.GetLifetimeService();
        }

        public object InitializeLifetimeService()
        {
            return _sqlConnection.InitializeLifetimeService();
        }

        public ObjRef CreateObjRef(Type requestedType)
        {
            return _sqlConnection.CreateObjRef(requestedType);
        }

        public void Dispose()
        {
            _sqlConnection.Dispose();
        }

        public ISite Site
        {
            get { return _sqlConnection.Site; }
            set { _sqlConnection.Site = value; }
        }

        public IContainer Container
        {
            get { return _sqlConnection.Container; }
        }

        public event EventHandler Disposed
        {
            add { _sqlConnection.Disposed += value; }
            remove { _sqlConnection.Disposed -= value; }
        }

        public Task OpenAsync()
        {
            return _sqlConnection.OpenAsync();
        }

        public event StateChangeEventHandler StateChange
        {
            add { _sqlConnection.StateChange += value; }
            remove { _sqlConnection.StateChange -= value; }
        }

        public object Clone()
        {
            return ((ICloneable) _sqlConnection).Clone();
        }

        public IDbTransaction BeginTransaction()
        {
            return _sqlConnection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel iso)
        {
            return _sqlConnection.BeginTransaction(iso);
        }

        public SqlTransaction BeginTransaction(string transactionName)
        {
            return _sqlConnection.BeginTransaction(transactionName);
        }

        public SqlTransaction BeginTransaction(IsolationLevel iso, string transactionName)
        {
            return _sqlConnection.BeginTransaction(iso, transactionName);
        }

        public void ChangeDatabase(string database)
        {
            _sqlConnection.ChangeDatabase(database);
        }

        public void Close()
        {
            _sqlConnection.Close();
        }

        public IDbCommand CreateCommand()
        {
            return _sqlConnection.CreateCommand();
        }

        public void Open()
        {
            _sqlConnection.Open();
        }

        public Task OpenAsync(CancellationToken cancellationToken)
        {
            return _sqlConnection.OpenAsync(cancellationToken);
        }

        public void ResetStatistics()
        {
            _sqlConnection.ResetStatistics();
        }

        public IDictionary RetrieveStatistics()
        {
            return _sqlConnection.RetrieveStatistics();
        }

        public DataTable GetSchema()
        {
            return _sqlConnection.GetSchema();
        }

        public DataTable GetSchema(string collectionName)
        {
            return _sqlConnection.GetSchema(collectionName);
        }

        public DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return _sqlConnection.GetSchema(collectionName, restrictionValues);
        }

        public bool StatisticsEnabled
        {
            get { return _sqlConnection.StatisticsEnabled; }
            set { _sqlConnection.StatisticsEnabled = value; }
        }

        public string ConnectionString
        {
            get { return _sqlConnection.ConnectionString; }
            set { _sqlConnection.ConnectionString = value; }
        }

        public int ConnectionTimeout
        {
            get { return _sqlConnection.ConnectionTimeout; }
        }

        public string Database
        {
            get { return _sqlConnection.Database; }
        }

        public string DataSource
        {
            get { return _sqlConnection.DataSource; }
        }

        public int PacketSize
        {
            get { return _sqlConnection.PacketSize; }
        }

        public Guid ClientConnectionId
        {
            get { return _sqlConnection.ClientConnectionId; }
        }

        public string ServerVersion
        {
            get { return _sqlConnection.ServerVersion; }
        }

        public string WorkstationId
        {
            get { return _sqlConnection.WorkstationId; }
        }

        public SqlCredential Credential
        {
            get { return _sqlConnection.Credential; }
            set { _sqlConnection.Credential = value; }
        }

        public bool FireInfoMessageEventOnUserErrors
        {
            get { return _sqlConnection.FireInfoMessageEventOnUserErrors; }
            set { _sqlConnection.FireInfoMessageEventOnUserErrors = value; }
        }

        public ConnectionState State
        {
            get { return _sqlConnection.State; }
        }

        public event SqlInfoMessageEventHandler InfoMessage
        {
            add { _sqlConnection.InfoMessage += value; }
            remove { _sqlConnection.InfoMessage -= value; }
        }

        public SqlConnectionAsync(SqlConnection sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }
    }
}