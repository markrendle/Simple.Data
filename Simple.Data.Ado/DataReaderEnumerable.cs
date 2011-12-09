using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Threading;

    internal class DataReaderEnumerable : IEnumerable<IDictionary<string, object>>
    {
        private IEnumerable<IDictionary<string, object>> _cache; 
        private IDictionary<string, int> _index;
        private readonly IDbCommand _command;
        private readonly Func<IDbConnection> _createConnection;

        public DataReaderEnumerable(IDbCommand command, Func<IDbConnection> createConnection)
            : this(command, createConnection, null)
        {
        }

        public DataReaderEnumerable(IDbCommand command, Func<IDbConnection> createConnection, IDictionary<string, int> index)
        {
            _command = command;
            _createConnection = createConnection;
            _index = index;
        }

        public IEnumerator<IDictionary<string, object>> GetEnumerator()
        {
            if (_cache != null) return _cache.GetEnumerator();

            IDbCommand command;

            var clonable = _command as ICloneable;
            if (clonable != null)
            {
                command = (IDbCommand) clonable.Clone();
                command.Connection = _createConnection();
            }
            else
            {
                command = _command;
            }

            return new DataReaderEnumerator(command, _index, Cache, CacheIndex);
        }

        private void Cache(IEnumerable<IDictionary<string,object>> cache)
        {
            Interlocked.CompareExchange(ref _cache, cache, null);
        }

        private void CacheIndex(IDictionary<string,int> index)
        {
            Interlocked.CompareExchange(ref _index, index, null);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class DataReaderEnumerator : IEnumerator<IDictionary<string, object>>
        {
            private readonly IDisposable _connectionDisposable;
            private IDictionary<string, int> _index;
            private readonly IDbCommand _command;
            private IList<IDictionary<string,object>> _cache = new List<IDictionary<string, object>>(); 
            private readonly Action<IEnumerable<IDictionary<string, object>>> _cacheAction;
            private readonly Action<IDictionary<string, int>> _cacheIndexAction;
            private IDataReader _reader;
            private IDictionary<string, object> _current;

            public DataReaderEnumerator(IDbCommand command, IDictionary<string, int> index, Action<IEnumerable<IDictionary<string, object>>> cacheAction, Action<IDictionary<string, int>> cacheIndexAction)
            {
                _command = command;
                _cacheAction = cacheAction;
                _cacheIndexAction = cacheIndexAction;
                _connectionDisposable = _command.Connection.MaybeDisposable();
                _index = index;
            }

            public void Dispose()
            {
                using (_connectionDisposable)
                using (_command)
                using (_reader)
                {
                    /* NO-OP */
                }
            }

            public bool MoveNext()
            {
                if (_reader == null)
                {
                    ExecuteReader();
                    if (_reader == null) return false;
                }

                return _reader.Read() ? SetCurrent() : EndRead();
            }

            private bool SetCurrent()
            {
                _current = _reader.ToDictionary(_index);

                // We don't want to cache more than 100 rows, too much memory would be used.
                if (_cache.Count < 100)
                {
                    _cache.Add(_current);
                }
                else
                {
                    _cache = null;
                }

                return true;
            }

            private bool EndRead()
            {
                _current = null;

                // When reader is done, cache the results to the DataReaderEnumerable.
                if (_cache != null)
                {
                    _cacheAction(_cache);
                }

                return false;
            }

            private void ExecuteReader()
            {
                try
                {
                    _command.Connection.OpenIfClosed();
                    _reader = _command.ExecuteReader();
                    CreateIndexIfNecessary();
                }
                catch (DbException ex)
                {
                    throw new AdoAdapterException(ex.Message, ex);
                }
            }

            private void CreateIndexIfNecessary()
            {
                if (_reader != null && _index == null)
                {
                    _index = _reader.CreateDictionaryIndex();
                    _cacheIndexAction(_index);
                }
            }

            public void Reset()
            {
                if (_reader != null) _reader.Dispose();
                _cache.Clear();
                ExecuteReader();
            }

            public IDictionary<string, object> Current
            {
                get
                {
                    if (_current == null) throw new InvalidOperationException();
                    return _current;
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}