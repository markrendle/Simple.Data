using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    using System.Collections;
    using System.Data;
    using System.Data.Common;

    class DataReaderEnumerator : IEnumerator<IDictionary<string,object>>
    {
        private readonly IDbConnection _connection;
        private IDictionary<string, int> _index;
        private readonly IDbCommand _command;
        private IDataReader _reader;
        private bool _lastRead;

        public DataReaderEnumerator(IDbCommand command, IDbConnection connection) : this(command, connection, null)
        {
        }

        public DataReaderEnumerator(IDbCommand command, IDbConnection connection, IDictionary<string,int> index)
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
            {  /* NO-OP */ }
        }

        public bool MoveNext()
        {
            if (_reader == null)
            {
                ExecuteReader();
            }
            return _lastRead = _reader.Read();
        }

        private void ExecuteReader()
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();
                _reader = _command.ExecuteReader();
                _index = _index ?? _reader.CreateDictionaryIndex();
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
                if (!_lastRead) throw new InvalidOperationException();
                return _reader.ToDictionary();
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
