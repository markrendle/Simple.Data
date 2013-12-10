using System.Data;
using NUnit.Framework;

namespace Simple.Data.Ado.Test
{
    using System;

    [TestFixture]
    public class ConnectionModifierTest
    {
        [Test]
        public void ModifiesConnection()
        {
            var adapter = new AdoAdapter(new StubConnectionProvider());
            adapter.SetConnectionModifier(c => new FooConnection(c));
            Assert.IsInstanceOf<FooConnection>(adapter.CreateConnection());
        }

        [Test]
        public void ClearsConnection()
        {
            var adapter = new AdoAdapter(new StubConnectionProvider());
            adapter.SetConnectionModifier(c => new FooConnection(c));
            Assert.IsInstanceOf<FooConnection>(adapter.CreateConnection());
            adapter.ClearConnectionModifier();
            Assert.IsNotInstanceOf<FooConnection>(adapter.CreateConnection());
        }

        [Test]
        public void ConnectionCreatedEventFires()
        {
            bool fired = false;
            EventHandler<ConnectionCreatedEventArgs> handler = (o, e) => { fired = true; };
            AdoAdapter.ConnectionCreated += handler;
            var adapter = new AdoAdapter(new StubConnectionProvider());
            var connection = adapter.CreateConnection();
            AdoAdapter.ConnectionCreated -= handler;
            Assert.True(fired);
        }
        
        [Test]
        public void ConnectionCreatedCanOverrideConnection()
        {
            EventHandler<ConnectionCreatedEventArgs> handler = (o, e) => e.OverrideConnection(new BarConnection(e.Connection));
            AdoAdapter.ConnectionCreated += handler;
            var adapter = new AdoAdapter(new StubConnectionProvider());
            var connection = adapter.CreateConnection();
            Assert.IsInstanceOf<BarConnection>(connection);
            AdoAdapter.ConnectionCreated -= handler;
        }

        private class FooConnection : IDbConnection
        {
            private readonly IDbConnection _wrapped;

            public FooConnection(IDbConnection wrapped)
            {
                _wrapped = wrapped;
            }

            public void Dispose()
            {
                throw new System.NotImplementedException();
            }

            public IDbTransaction BeginTransaction()
            {
                throw new System.NotImplementedException();
            }

            public IDbTransaction BeginTransaction(IsolationLevel il)
            {
                throw new System.NotImplementedException();
            }

            public void Close()
            {
                throw new System.NotImplementedException();
            }

            public void ChangeDatabase(string databaseName)
            {
                throw new System.NotImplementedException();
            }

            public IDbCommand CreateCommand()
            {
                throw new System.NotImplementedException();
            }

            public void Open()
            {
                throw new System.NotImplementedException();
            }

            public string ConnectionString { get; set; }
            public int ConnectionTimeout { get; private set; }
            public string Database { get; private set; }
            public ConnectionState State { get; private set; }
        }
        
        private class BarConnection : IDbConnection
        {
            private readonly IDbConnection _wrapped;

            public BarConnection(IDbConnection wrapped)
            {
                _wrapped = wrapped;
            }

            public void Dispose()
            {
                throw new System.NotImplementedException();
            }

            public IDbTransaction BeginTransaction()
            {
                throw new System.NotImplementedException();
            }

            public IDbTransaction BeginTransaction(IsolationLevel il)
            {
                throw new System.NotImplementedException();
            }

            public void Close()
            {
                throw new System.NotImplementedException();
            }

            public void ChangeDatabase(string databaseName)
            {
                throw new System.NotImplementedException();
            }

            public IDbCommand CreateCommand()
            {
                throw new System.NotImplementedException();
            }

            public void Open()
            {
                throw new System.NotImplementedException();
            }

            public string ConnectionString { get; set; }
            public int ConnectionTimeout { get; private set; }
            public string Database { get; private set; }
            public ConnectionState State { get; private set; }
        }
    }
}