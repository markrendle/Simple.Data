using System;
using System.Linq;
using System.Data;

namespace Simple.Data.Mocking.Ado
{
    class MockDbCommand : IDbCommand
    {
        private readonly MockDbConnection _connection;

        public MockDbCommand(MockDbConnection connection)
        {
            _connection = connection;
        }

        private readonly MockDataParameterCollection _parameters = new MockDataParameterCollection();

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public string CommandText { get; set; }

        public int CommandTimeout { get; set; }

        public CommandType CommandType { get; set; }

        public IDbConnection Connection { get; set; }

        public IDbDataParameter CreateParameter()
        {
            return new MockDataParameter();
        }

        public int ExecuteNonQuery()
        {
            MockDatabase.Record(this);
            return 1;
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return ExecuteReader();
        }

        public IDataReader ExecuteReader()
        {
            MockDatabase.Record(this);
            if (_connection != null && _connection.DummyDataTable != null)
            {
                return _connection.DummyDataTable.CreateDataReader();
            }
            return new MockDataReader(Enumerable.Empty<IDataRecord>());
        }

        public object ExecuteScalar()
        {
            MockDatabase.Record(this);
            return null;
        }

        public IDataParameterCollection Parameters
        {
            get { return _parameters; }
        }

        public void Prepare()
        {
            throw new NotImplementedException();
        }

        public IDbTransaction Transaction
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public UpdateRowSource UpdatedRowSource
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            
        }
    }
}
