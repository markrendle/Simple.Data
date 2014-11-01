using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    using System.Threading.Tasks;

    public interface ICustomInserter
    {
        Task<IDictionary<string, object>> Insert(AdoAdapter adapter, string tableName, IDictionary<string, object> data, IDbTransaction transaction = null, bool resultRequired = false);
    }
}
