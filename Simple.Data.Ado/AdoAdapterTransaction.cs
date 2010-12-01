using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    class AdoAdapterTransaction : IAdapterTransaction
    {
        private readonly string _name;
        private readonly DbTransaction _dbTransaction;

        public AdoAdapterTransaction(DbTransaction dbTransaction) : this(dbTransaction, null)
        {
        }

        public AdoAdapterTransaction(DbTransaction dbTransaction, string name)
        {
            _name = name;
            _dbTransaction = dbTransaction;
        }

        internal DbTransaction Transaction
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
