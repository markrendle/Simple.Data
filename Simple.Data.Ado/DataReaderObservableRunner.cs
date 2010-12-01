using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Simple.Data.Ado
{
    class DataReaderObservableRunner : IDisposable
    {
        private readonly IDbCommand _command;
        private readonly IObserver<IDictionary<string, object>> _observer;
        private bool _disposed;

        private DataReaderObservableRunner(IDbCommand command, IObserver<IDictionary<string, object>> observer)
        {
            _command = command;
            _observer = observer;
        }

        public static DataReaderObservableRunner RunAsync(IDbCommand command, IObserver<IDictionary<string, object>> observer)
        {
            var instance = new DataReaderObservableRunner(command, observer);
            var asyncAction = new Action(instance.Run);
            asyncAction.BeginInvoke(asyncAction.EndInvoke, null);
            return instance;
        }

        private void Run()
        {
            try
            {
                RunQuery();
                SendCompleted();
            }
            catch (Exception ex)
            {
                _observer.OnError(ex);
            }
        }

        private void RunQuery()
        {
            if (_command.Connection.State == ConnectionState.Open)
            {
                RunReader();
            }
            else
            {
                using (_command.Connection)
                using (_command)
                {
                    _command.Connection.Open();
                    RunReader();
                }
            }
        }

        private void RunReader()
        {
            using (var reader = _command.ExecuteReader())
            {
                while ((!_disposed) && reader.Read())
                {
                    _observer.OnNext(reader.ToDictionary());
                }
            }
        }

        private void SendCompleted()
        {
            if (!_disposed)
            {
                _observer.OnCompleted();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _disposed = true;
        }
    }
}