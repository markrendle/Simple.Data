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

        public AdoAdapterTransaction(IDbTransaction dbTransaction) : this(dbTransaction, null)
        {
        }

        public AdoAdapterTransaction(IDbTransaction dbTransaction, string name)
        {
            _name = name;
            _dbTransaction = dbTransaction;
        }

        internal IDbTransaction Transaction
        {
            get { return _dbTransaction; }
        }

        public void Dispose()
        {
            _dbTransaction.Dispose();
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
