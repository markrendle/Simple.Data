using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    public interface ICustomInserter
    {
        IDictionary<string, object> Insert(AdoAdapter adapter, string tableName, IDictionary<string, object> data);
    }
}
