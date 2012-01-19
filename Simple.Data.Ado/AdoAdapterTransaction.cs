using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    class AdoAdapterTransaction : IAdapterTransaction
    {
        private readonly string _name;
        private readonly IDbTransaction _dbTransaction;
        private readonly IDbConnection _dbConnection;
        private readonly bool _sharedConnection;

        public AdoAdapterTransaction(IDbTransaction dbTransaction, bool sharedConnection = false) : this(dbTransaction, null, sharedConnection)
        {
        }

        public AdoAdapterTransaction(IDbTransaction dbTransaction, string name, bool sharedConnection = false)
        {
            _name = name;
            _dbTransaction = dbTransaction;
            _dbConnection = _dbTransaction.Connection;
            _sharedConnection = sharedConnection;
        }

        internal IDbTransaction Transaction
        {
            get { return _dbTransaction; }
        }

        public void Dispose()
        {
            _dbTransaction.Dispose();
            if (!_sharedConnection)
                _dbConnection.Dispose();
        }

        public void Commit()
        {
            _dbTransaction.Commit();
        }

        public void Rollback()
        {
            _dbTransaction.Rollback();
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
