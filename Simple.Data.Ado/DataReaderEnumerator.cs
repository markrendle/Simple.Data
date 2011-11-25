using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    using System.Collections;
    using System.Data;
    using System.Data.Common;

    internal class DataReaderEnumerator : IEnumerator<IDictionary<string, object>>
    {
        private readonly IDbConnection _connection;
        private IDictionary<string, int> _index;
        private readonly IDbCommand _command;
        private IDataReader _reader;
        private bool _lastRead;

        public DataReaderEnumerator(IDbCommand command, IDbConnection connection)
            : this(command, connection, null)
        {
        }

        public DataReaderEnumerator(IDbCommand command, IDbConnection connection, IDictionary<string, int> index)
        {
            _command = command;
            _connection = connection;
            _index = index;
        }

        public void Dispose()
        {
            using (_connection)
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
            }
            return _lastRead = (_reader != null && _reader.Read());
        }

        private void ExecuteReader()
        {
            try
            {
                _connection.OpenIfClosed();
                _reader = _command.ExecuteReader();
                if (_reader != null)
                {
                    _index = _index ?? _reader.CreateDictionaryIndex();
                }
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, ex);
            }
        }

        public void Reset()
        {
            if (_reader != null) _reader.Dispose();
            ExecuteReader();
        }

        public IDictionary<string, object> Current
        {
            get
            {
                if (_reader == null)
                {
                    return null;
                }
                if (!_lastRead) throw new InvalidOperationException();
                return _reader.ToDictionary(_index);
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}