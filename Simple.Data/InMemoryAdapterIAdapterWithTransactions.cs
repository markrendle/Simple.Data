using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Operations;

namespace Simple.Data
{
    public partial class InMemoryAdapter : IAdapterWithTransactions
    {
        class InMemoryAdapterTransaction : IAdapterTransaction
        {
            private readonly string _name;

            public InMemoryAdapterTransaction() : this(string.Empty)
            {
            }

            public InMemoryAdapterTransaction(string name)
            {
                _name = name;
            }

            public void Dispose()
            {
            }

            public void Commit()
            {
            }

            public void Rollback()
            {
            }

            public string Name
            {
                get { return _name; }
            }
        }

        public IAdapterTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            return new InMemoryAdapterTransaction();
        }

        public IAdapterTransaction BeginTransaction(string name, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            return new InMemoryAdapterTransaction(name);
        }
    }
}
