using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.Ado
{
    using System.Data;

    class ConnectionScope : IDisposable
    {
        private readonly IDbConnection _connection;

        public IDbConnection Connection
        {
            get { return _connection; }
        }

        private readonly bool _dispose;

        private ConnectionScope(IDbConnection connection, bool dispose)
        {
            _connection = connection;
            _dispose = dispose;
        }

        public static ConnectionScope Create(IDbTransaction transaction, Func<IDbConnection> creator)
        {
            if (transaction != null)
            {
                return new ConnectionScope(transaction.Connection, false);
            }
            var connection = creator();
            connection.OpenIfClosed();
            return new ConnectionScope(connection, true);
        }

        public void Dispose()
        {
            if (!_dispose) return;
            _connection.Dispose();
        }
    }
}
