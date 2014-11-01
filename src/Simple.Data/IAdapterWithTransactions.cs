using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Operations;

namespace Simple.Data
{
    using System.Threading.Tasks;

    public interface IAdapterWithTransactions
    {
        IAdapterTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        IAdapterTransaction BeginTransaction(string name, IsolationLevel isolationLevel = IsolationLevel.Unspecified);

        Task<OperationResult> Execute(IOperation operation, IAdapterTransaction transaction);
    }
}
