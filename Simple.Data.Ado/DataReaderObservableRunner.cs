using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Simple.Data.Ado
{
    class DataReaderObservableRunner : IDisposable
    {
        private static readonly object StaticSync = new object();
        private static readonly WeakCache<DataReaderObservableRunner> RunnerCache = new WeakCache<DataReaderObservableRunner>();

        private readonly IDbCommand _command;
        private readonly IObserver<IDictionary<string, object>> _observer;
        private IList<IDictionary<string, object>> _cache;
        private bool _disposed;

        protected DataReaderObservableRunner()
        {
            
        }

        private DataReaderObservableRunner(IDbCommand command, IObserver<IDictionary<string, object>> observer)
        {
            _command = command;
            _observer = observer;
        }

        public static DataReaderObservableRunner RunAsync(IDbCommand command, IObserver<IDictionary<string, object>> observer)
        {
            DataReaderObservableRunner instance = null;

            if (!RunnerCache.Contains(command))
            {
                instance = CreateInstance(command, observer);
            }

            if (instance == null) instance = new CachedRunner(RunnerCache.Get(command), observer);
            var asyncAction = new Action(instance.Run);
            asyncAction.BeginInvoke(asyncAction.EndInvoke, null);
            return instance;
        }

        private static DataReaderObservableRunner CreateInstance(IDbCommand command, IObserver<IDictionary<string, object>> observer)
        {
            lock (StaticSync)
            {
                if (!RunnerCache.Contains(command))
                {
                    var instance = new DataReaderObservableRunner(command, observer);
                    RunnerCache.Add(command, instance);
                    return instance;
                }
            }
            return null;
        }

        protected virtual void Run()
        {
            try
            {
                RunQuery();
                _observer.OnCompleted();
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
            _cache = new List<IDictionary<string, object>>();
            using (var reader = _command.ExecuteReader())
            {
                while ((!_disposed) && reader.Read())
                {
                    var row = reader.ToDictionary();
                    _cache.Add(row);
                    _observer.OnNext(row);
                }
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

        private class CachedRunner : DataReaderObservableRunner
        {
            private readonly DataReaderObservableRunner _original;
            private readonly IObserver<IDictionary<string, object>> _newObserver;

            public CachedRunner(DataReaderObservableRunner original, IObserver<IDictionary<string, object>> observer)
            {
                _original = original;
                _newObserver = observer;
            }

            protected override void Run()
            {
                foreach (var row in _original._cache)
                {
                    _newObserver.OnNext(row);
                }
                _newObserver.OnCompleted();
            }
        }
    }
}