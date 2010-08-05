using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Simple.Data.Test.Stubs
{
    class DbCommandStub : IDbCommand
    {
        private readonly DataParameterCollection _parameters = new DataParameterCollection();

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
            return new DataParameterStub();
        }

        public int ExecuteNonQuery()
        {
            DatabaseStub.Record(this);
            return 1;
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return ExecuteReader();
        }

        public IDataReader ExecuteReader()
        {
            DatabaseStub.Record(this);
            return new DataReaderStub(Enumerable.Empty<IDataRecord>());
        }

        public object ExecuteScalar()
        {
            DatabaseStub.Record(this);
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
